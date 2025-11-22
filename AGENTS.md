# Repository Guidelines

## Project Structure & Module Organization
- Place application code in `src/`, organized by domain. Shared helpers belong in `src/lib/` and cross-cutting UI in `src/components/`.
- Keep tests in `tests/`, mirroring the source paths (e.g., `src/api/payments.ts` -> `tests/api/payments.test.ts`).
- Use `scripts/` for developer automation and `docs/` for specifications or ADRs. Store static assets in `public/` or `assets/` depending on the toolchain.

## Build, Test, and Development Commands
- Install dependencies via the detected lockfile: `pnpm install` if `pnpm-lock.yaml`, `npm ci` if `package-lock.json`, or `pip install -r requirements.txt` for Python utilities. Prefer `make setup` if a `Makefile` is present.
- Run the app locally with `make dev` or the package script (`npm run dev`, `pnpm dev`) defined in `package.json`.
- Execute verification before pushing: `make test` (or `npm test` / `pytest tests/`) plus `make lint` (or `npm run lint` / `ruff check`). Use `make format` or `npm run format` to apply auto-formatting.

## Coding Style & Naming Conventions
- Default to 2-space indentation for JS/TS and 4-space for Python; keep lines under ~100 characters.
- Components and classes use PascalCase; functions and variables use camelCase; environment/config keys use UPPER_SNAKE_CASE.
- Keep modules cohesive and avoid cyclic dependencies; favor small, single-responsibility files. Run Prettier/ESLint for JS/TS or Black/Ruff for Python before committing.

## Testing Guidelines
- Mirror production code paths under `tests/` (e.g., `tests/components/Button.test.tsx`, `tests/api/test_payments.py`).
- Target meaningful coverage (≥80% when practical). Add regression tests when fixing bugs—reproduce first, then fix.
- Name tests descriptively (`it handles insufficient balance`) and favor fixtures/builders for shared setup.

## Commit & Pull Request Guidelines
- Follow Conventional Commits: `feat: add payout caps`, `fix: handle nil balances`, `chore: bump lint rules`. One logical change per PR.
- PRs should link related issues, include screenshots for UI changes, and call out breaking changes explicitly (use `BREAKING CHANGE:` in the commit footer when applicable).
- Ensure CI checks pass (`make test lint`) and update docs/tests alongside code changes.

## Security & Configuration Tips
- Never commit secrets; use `.env.local` for private values and `.env.example` for safe defaults. Load env vars via your process manager instead of hard-coding.
- Validate all external input, sanitize user-provided data, and review dependency changes before merging.
