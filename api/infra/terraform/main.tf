locals {
  name_prefix = var.project_name
}

# ----------------------------------------
# Key Pair
# ----------------------------------------
resource "aws_key_pair" "this" {
  key_name   = "${local.name_prefix}-key"
  public_key = file(var.public_key_path)

  tags = {
    Name = "${local.name_prefix}-key"
  }
}

# ----------------------------------------
# Security Group
# ----------------------------------------
data "aws_vpc" "default" {
  default = true
}

data "aws_subnets" "default" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.default.id]
  }
}

resource "aws_security_group" "this" {
  name        = "${local.name_prefix}-sg"
  description = "Security group for ${local.name_prefix} API"
  vpc_id      = data.aws_vpc.default.id

  # SSH (port 22) - kept for safety/rescue
  ingress {
    description = "SSH (fallback)"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = [var.ssh_allowed_cidr]
  }

  # SSH custom port (50022)
  ingress {
    description = "SSH custom"
    from_port   = var.ssh_port
    to_port     = var.ssh_port
    protocol    = "tcp"
    cidr_blocks = [var.ssh_allowed_cidr]
  }

  # HTTP
  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # HTTPS
  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Egress: allow all outbound
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${local.name_prefix}-sg"
  }
}

# ----------------------------------------
# EC2 Instance
# ----------------------------------------
resource "aws_instance" "this" {
  ami                         = var.ami_id
  instance_type               = var.instance_type
  subnet_id                   = data.aws_subnets.default.ids[0]
  associate_public_ip_address = true

  key_name               = aws_key_pair.this.key_name
  vpc_security_group_ids = [aws_security_group.this.id]

  # Safe, idempotent bootstrap script
  user_data = file("${path.module}/userdata.sh")

  tags = {
    Name = "${local.name_prefix}-api"
    Role = "cashify-api"
  }

  lifecycle {
    create_before_destroy = false
  }
}
