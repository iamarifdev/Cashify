#!/bin/bash

# EF Core Migration Management Script
# Usage: ./ef-migrations.sh [command] [migration_name] [options]

set -e

# Default values
PROJECT="Cashify.Api"
STARTUP_PROJECT="Cashify.Api"
CONTEXT="AppDbContext"
MIGRATIONS_DIR="Database/Migrations"
SQL_DIR="database"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to get the next SQL script number
get_next_sql_number() {
    if [ ! -d "$SQL_DIR" ]; then
        mkdir -p "$SQL_DIR"
        echo "01"
        return
    fi
    
    local last_number=$(ls "$SQL_DIR"/ | grep -E '^[0-9]+_' | sort -V | tail -1 | grep -oE '^[0-9]+' || echo "00")
    local next_number=$((10#$last_number + 1))
    printf "%02d" $next_number
}

# Function to get the last migration name and timestamp
get_last_migration() {
    local migrations_path="$PROJECT/$MIGRATIONS_DIR"
    if [ ! -d "$migrations_path" ]; then
        echo ""
        return
    fi
    
    # Find the latest migration file by timestamp (exclude Designer files and snapshot)
    local last_migration_file=$(ls "$migrations_path"/*.cs 2>/dev/null | \
        grep -v "\.Designer\.cs$" | \
        grep -v "ModelSnapshot\.cs$" | \
        sort -V | tail -1)
    
    if [ -n "$last_migration_file" ]; then
        # Extract timestamp and migration name from filename
        basename "$last_migration_file" | sed 's/\.cs$//' | sed 's/^[0-9]*_//'
    fi
}

# Function to get the last migration full name (timestamp_name)
get_last_migration_full_name() {
    local migrations_path="$PROJECT/$MIGRATIONS_DIR"
    if [ ! -d "$migrations_path" ]; then
        echo ""
        return
    fi
    
    # Find the latest migration file by timestamp
    local last_migration_file=$(ls "$migrations_path"/*.cs 2>/dev/null | \
        grep -v "\.Designer\.cs$" | \
        grep -v "ModelSnapshot\.cs$" | \
        sort -V | tail -1)
    
    if [ -n "$last_migration_file" ]; then
        # Extract full name (timestamp_name) from filename
        basename "$last_migration_file" | sed 's/\.cs$//'
    fi
}

# Function to get the previous migration for incremental SQL generation
get_previous_migration() {
    local current_migration="$1"
    local migrations_path="$PROJECT/$MIGRATIONS_DIR"
    
    if [ ! -d "$migrations_path" ]; then
        echo ""
        return
    fi
    
    # Get all migration files sorted by timestamp, exclude the current one
    local previous_migration=$(ls "$migrations_path"/*.cs 2>/dev/null | \
        grep -v "\.Designer\.cs$" | \
        grep -v "ModelSnapshot\.cs$" | \
        grep -v "$current_migration" | \
        sort -V | tail -1)
    
    if [ -n "$previous_migration" ]; then
        basename "$previous_migration" .cs
    fi
}

# Function to show help
show_help() {
    cat << EOF
EF Core Migration Management Script

Usage:
    ./ef-migrations.sh add <migration_name> [--no-sql]     Add a new migration (auto-generates SQL)
    ./ef-migrations.sh update                              Update database to latest migration
    ./ef-migrations.sh remove                              Remove the last migration
    ./ef-migrations.sh script [migration_name]             Generate SQL script for last/specific migration
    ./ef-migrations.sh list                                List all migrations
    ./ef-migrations.sh help                                Show this help message

Examples:
    ./ef-migrations.sh add UpdateUser                      # Add migration + auto-generate SQL
    ./ef-migrations.sh add CreateProductTable --no-sql     # Add migration without SQL
    ./ef-migrations.sh update                              # Update database
    ./ef-migrations.sh remove                              # Remove last migration
    ./ef-migrations.sh script                             # Generate SQL for last migration
    ./ef-migrations.sh script UpdateUser                   # Generate SQL for specific migration

Generated SQL files will be saved to the 'database' directory with auto-numbered prefixes:
- 01_InitialCreate.sql
- 02_UpdateUser.sql
- 03_CreateProductTable.sql
- etc.

Note: The script automatically detects migration files with timestamp prefixes
(e.g., 20251213215350_UpdateUser.cs) and uses the actual migration name for SQL files.

EOF
}

# Function to add a migration
add_migration() {
    local migration_name="$1"
    local auto_generate_sql="$2"
    
    if [ -z "$migration_name" ]; then
        print_error "Migration name is required"
        echo "Usage: ./ef-migrations.sh add <migration_name> [--no-sql]"
        exit 1
    fi
    
    print_status "Adding migration: $migration_name"
    
    dotnet ef migrations add "$migration_name" \
        --project "$PROJECT" \
        --startup-project "$STARTUP_PROJECT" \
        -c "$CONTEXT" \
        -o "$MIGRATIONS_DIR"
    
    if [ $? -eq 0 ]; then
        print_success "Migration '$migration_name' added successfully"
        
        # Auto-generate SQL script for the newly created migration unless --no-sql is specified
        if [ "$auto_generate_sql" != "--no-sql" ]; then
            print_status "Auto-generating SQL script for the new migration..."
            sleep 2  # Give a moment for the migration files to be fully created
            generate_sql_for_migration "$migration_name"
        else
            print_status "SQL script generation skipped as requested"
        fi
    else
        print_error "Failed to add migration"
        exit 1
    fi
}

# Function to update database
update_database() {
    print_status "Updating database to latest migration"
    
    dotnet ef database update \
        --project "$PROJECT" \
        --startup-project "$STARTUP_PROJECT" \
        -c "$CONTEXT"
    
    if [ $? -eq 0 ]; then
        print_success "Database updated successfully"
    else
        print_error "Failed to update database"
        exit 1
    fi
}

# Function to remove the last migration
remove_migration() {
    print_status "Removing the last migration"
    
    dotnet ef migrations remove \
        --project "$PROJECT" \
        --startup-project "$STARTUP_PROJECT" \
        -c "$CONTEXT"
    
    if [ $? -eq 0 ]; then
        print_success "Last migration removed successfully"
    else
        print_error "Failed to remove migration"
        exit 1
    fi
}

# Function to generate SQL script for a specific migration
generate_sql_for_migration() {
    local migration_name="$1"
    local sql_number=$(get_next_sql_number)
    
    if [ -z "$migration_name" ]; then
        print_error "Migration name is required for SQL generation"
        return 1
    fi
    
    # Convert migration name to snake_case for filename
    local script_name="${sql_number}_${migration_name//[_ ]/_}.sql"
    
    # Ensure database directory exists
    mkdir -p "$SQL_DIR"
    
    print_status "Generating SQL script for migration: $migration_name"
    
    # Get the full migration name (timestamp_name) from the file system
    local full_migration_name
    local migrations_path="$PROJECT/$MIGRATIONS_DIR"
    
    # Try to find the migration file that matches the name
    local migration_file=$(ls "$migrations_path"/*_"$migration_name".cs 2>/dev/null | head -1)
    if [ -n "$migration_file" ]; then
        full_migration_name=$(basename "$migration_file" .cs)
    else
        # If not found, try to get the last migration
        full_migration_name=$(get_last_migration_full_name)
        if [ -z "$full_migration_name" ]; then
            print_error "Could not find migration file for: $migration_name"
            return 1
        fi
    fi
    
    # Get the previous migration for incremental generation
    local previous_migration=$(get_previous_migration "$full_migration_name")
    
    # Generate SQL script for the specific migration and capture output
    local temp_file=$(mktemp)
    
    # Build the script command
    if [ -n "$previous_migration" ]; then
        # Generate incremental SQL from previous migration to current one
        dotnet ef migrations script "$previous_migration" "$full_migration_name" \
            --project "$PROJECT" \
            --startup-project "$STARTUP_PROJECT" \
            -c "$CONTEXT" > "$temp_file" 2>&1
    else
        # This is the first migration, generate from scratch
        dotnet ef migrations script "$full_migration_name" \
            --project "$PROJECT" \
            --startup-project "$STARTUP_PROJECT" \
            -c "$CONTEXT" > "$temp_file" 2>&1
    fi
    
    # Filter out build output and keep only SQL content
    # Look for lines that start with SQL keywords or contain actual SQL
    sed -n '/^CREATE\|^ALTER\|^DROP\|^INSERT\|^UPDATE\|^DELETE\|^START TRANSACTION\|^COMMIT\|^BEGIN;/p' "$temp_file" > "$SQL_DIR/$script_name"
    
    # If no SQL content was found, try an alternative approach
    if [ ! -s "$SQL_DIR/$script_name" ]; then
        # Try filtering out build messages and keeping the rest
        grep -v "Build started\|Build succeeded\|warning\|error" "$temp_file" > "$SQL_DIR/$script_name"
    fi
    
    # Clean up temp file
    rm -f "$temp_file"
    
    # Check if the generated file has actual SQL content
    if [ -s "$SQL_DIR/$script_name" ] && grep -q "CREATE\|ALTER\|DROP\|INSERT" "$SQL_DIR/$script_name"; then
        print_success "SQL script generated: $SQL_DIR/$script_name"
        return 0
    else
        print_error "Failed to generate SQL script or no SQL content found"
        rm -f "$SQL_DIR/$script_name"  # Remove empty file
        return 1
    fi
}

# Function to generate SQL script for the last migration
generate_script() {
    local migration_name="$1"
    
    if [ -z "$migration_name" ]; then
        # Get the last migration name
        migration_name=$(get_last_migration)
        if [ -z "$migration_name" ]; then
            print_error "No migrations found to generate script for"
            exit 1
        fi
        print_status "Generating SQL script for the last migration: $migration_name"
    else
        print_status "Generating SQL script for migration: $migration_name"
    fi
    
    generate_sql_for_migration "$migration_name"
    if [ $? -ne 0 ]; then
        exit 1
    fi
}

# Function to list migrations
list_migrations() {
    print_status "Listing all migrations"
    
    dotnet ef migrations list \
        --project "$PROJECT" \
        --startup-project "$STARTUP_PROJECT" \
        -c "$CONTEXT"
}

# Main script logic
main() {
    case "$1" in
        "add")
            add_migration "$2" "$3"
            ;;
        "update")
            update_database
            ;;
        "remove")
            remove_migration
            ;;
        "script")
            generate_script "$2"
            ;;
        "list")
            list_migrations
            ;;
        "help"|"--help"|"-h"|"")
            show_help
            ;;
        *)
            print_error "Unknown command: $1"
            echo "Use './ef-migrations.sh help' for usage information"
            exit 1
            ;;
    esac
}

# Check if .NET CLI is available
if ! command -v dotnet &> /dev/null; then
    print_error "dotnet CLI is not installed or not in PATH"
    exit 1
fi

# Check if dotnet-ef tool is installed
if ! dotnet tool list --global | grep -q "dotnet-ef"; then
    print_warning "dotnet-ef tool is not installed globally"
    print_status "Installing dotnet-ef tool..."
    dotnet tool install --global dotnet-ef
fi

# Run main function with all arguments
main "$@"