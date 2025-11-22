# Cashify Web (Angular 20)

This directory hosts the Angular 20 + Angular Material + Tailwind frontend. Scaffold with your preferred package manager:

```bash
cd web
npm create @angular@latest cashify-web -- --routing --style=scss
cd cashify-web
npm install @angular/material @ngrx/store @ngrx/effects tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

Key modules to add:
- Auth (Google One Tap/OAuth, JWT interceptor)
- Businesses, Cashbooks, Transactions (dialogs, inline lookup creation, history drawer)
- Dashboard with summaries and charts

Use environment variables (`environment.ts`) to point to the API base URL and Google Client ID.
