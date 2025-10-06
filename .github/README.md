# GitHub Workflows

This repository uses Google's release-please for automated version management and releases.

## Workflows

### 1. CI (`ci.yml`)
- **Triggers**: Push to `main`/`develop` branches, Pull Requests
- **Purpose**: Run tests and build validation
- **Actions**:
  - Builds the solution
  - Runs all unit tests
  - Generates test reports

### 2. Release Please (`release-please.yml`)
- **Triggers**: Push to `main` branch, Manual dispatch
- **Purpose**: Automated version management and release creation
- **Actions**:
  - Monitors conventional commits
  - Creates release PRs automatically
  - Updates CHANGELOG.md
  - Manages version numbers

### 3. Build and Publish (`build-and-publish.yml`)
- **Triggers**: Git tags starting with `v*` (created by release-please)
- **Purpose**: Build and publish NuGet package
- **Actions**:
  - Builds the solution
  - Runs tests
  - Creates NuGet package
  - Publishes to NuGet.org

## Setup Instructions

### 1. NuGet API Key
To enable automatic publishing to NuGet, add your NuGet API key as a repository secret:

1. Go to [NuGet.org](https://www.nuget.org/) and create an API key
2. In your GitHub repository, go to Settings → Secrets and variables → Actions
3. Add a new repository secret named `NUGET_API_KEY` with your API key value

### 2. Publishing a New Version

#### Using Release Please (Recommended)
1. Make changes to your code
2. Use conventional commits in your PRs:
   - `feat:` for new features (minor version bump)
   - `fix:` for bug fixes (patch version bump)
   - `feat!:` or `fix!:` for breaking changes (major version bump)
3. Merge PRs to `main` branch
4. Release Please will automatically:
   - Create a release PR with updated version and CHANGELOG
   - When you merge the release PR, it will create a tag and trigger the publish workflow

#### Example Conventional Commits
```bash
git commit -m "feat: add new validation method"
git commit -m "fix: resolve timeout issue in STS service"
git commit -m "feat!: change API signature for better usability"
```

## How Release Please Works

1. **Monitor Commits**: Release Please monitors conventional commits on the main branch
2. **Create Release PR**: When there are changes, it creates a PR with:
   - Updated version number in `KPS.Core.csproj`
   - Updated `CHANGELOG.md` with all changes
   - Release notes
3. **Merge Release PR**: When you merge the release PR:
   - A git tag is created (e.g., `v1.2.0`)
   - The build and publish workflow is triggered
   - NuGet package is published automatically

## Configuration

The release-please configuration is in `release-please-config.json`:
- Monitors `src/KPS.Core` package
- Updates version in `KPS.Core.csproj`
- Maintains `CHANGELOG.md`
