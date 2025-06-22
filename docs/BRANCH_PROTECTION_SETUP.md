# Branch Protection Setup Guide

This guide explains how to set up branch protection rules for the Event Sourcing Banking Demo to ensure code quality and prevent direct commits to the main branch.

## 🎯 Overview

Branch protection enforces:
- **No direct commits** to main branch
- **Pull Request required** for all changes
- **Status checks must pass** before merging
- **Up-to-date branches** required before merge
- **Code review required** from at least one reviewer

## 🔧 GitHub Branch Protection Setup

### Step 1: Navigate to Repository Settings

1. Go to your repository on GitHub
2. Click **Settings** tab
3. Click **Branches** in the left sidebar

### Step 2: Add Branch Protection Rule

1. Click **Add rule** button
2. Configure the following settings:

#### Branch Name Pattern
```
main
```

#### Protect Matching Branches
- [x] **Restrict pushes that create files larger than 100 MB**
- [x] **Require a pull request before merging**
  - [x] **Require approvals**: 1
  - [x] **Dismiss stale reviews when new commits are pushed**
  - [x] **Require review from code owners** (if CODEOWNERS file exists)
  - [x] **Restrict reviews to users with write access**
  - [x] **Allow specified actors to bypass required pull requests** (leave empty)

#### Require Status Checks
- [x] **Require status checks to pass before merging**
- [x] **Require branches to be up to date before merging**

**Add these status checks:**
- `Code Quality & Linting`
- `Build and Test (ubuntu-latest)`
- `Build and Test (windows-latest)`
- `Security Scan`
- `API Documentation`
- `Quality Gate`

#### Additional Restrictions
- [x] **Restrict pushes that create matching branches**
- [x] **Allow force pushes** (unchecked - disabled)
- [x] **Allow deletions** (unchecked - disabled)

### Step 3: Save Protection Rule

Click **Create** to save the branch protection rule.

## 🚦 Status Checks Configuration

Our CI workflow creates these status checks that must pass:

### 1. Code Quality & Linting
- Code formatting verification (`dotnet format --verify-no-changes`)
- Static analysis with warnings as errors
- Build verification

### 2. Build and Test (Matrix)
- Ubuntu and Windows builds
- Unit test execution
- Integration test execution
- Code coverage analysis (≥60% threshold)

### 3. Security Scan
- Vulnerability scanning
- Dependency audit
- Security policy compliance

### 4. API Documentation
- XML documentation generation
- Swagger documentation validation

### 5. Quality Gate
- Overall quality verification
- All previous checks passed confirmation

## 👥 CODEOWNERS Setup (Optional)

Create a `.github/CODEOWNERS` file to automatically request reviews from specific team members:

```
# Global owners
* @your-username

# Event sourcing domain logic
/src/Domain/ @domain-expert @your-username

# Infrastructure components
/src/Infrastructure/ @infrastructure-expert @your-username

# API endpoints
/src/WebApi/ @api-expert @your-username

# Documentation
*.md @documentation-expert @your-username
/docs/ @documentation-expert @your-username

# CI/CD workflows
/.github/ @devops-expert @your-username
```

## 🔄 Workflow Integration

### Required Workflow File

Ensure your `.github/workflows/build.yml` includes all the jobs that are referenced in the status checks:

```yaml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  code-quality:
    name: Code Quality & Linting
    # ... job configuration
    
  build-and-test:
    name: Build and Test
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest]
    # ... job configuration
    
  security-scan:
    name: Security Scan
    # ... job configuration
    
  api-documentation:
    name: API Documentation
    # ... job configuration
    
  quality-gate:
    name: Quality Gate
    needs: [code-quality, build-and-test, security-scan, api-documentation]
    # ... job configuration
```

## 🧪 Testing Branch Protection

### 1. Test Direct Push Prevention

Try to push directly to main:
```bash
git checkout main
echo "test" >> README.md
git add README.md
git commit -m "test direct push"
git push origin main
```

**Expected Result**: Push should be rejected with a protection error.

### 2. Test PR Workflow

Create a feature branch and PR:
```bash
git checkout -b test/branch-protection
echo "# Test Branch Protection" >> test.md
git add test.md
git commit -m "test: add branch protection test"
git push origin test/branch-protection
```

Then create a PR via GitHub UI and verify:
- [ ] Status checks are triggered
- [ ] Merge button is disabled until checks pass
- [ ] Review is required before merge

### 3. Test Status Check Requirements

Create a PR with failing tests:
```bash
# Add a failing test
git checkout -b test/failing-checks
# ... make changes that break tests
git push origin test/failing-checks
```

**Expected Result**: PR should show failing status checks and prevent merge.

## 🔧 Troubleshooting

### Status Checks Not Appearing

1. **Check Workflow Names**: Ensure job names in workflow match status check names
2. **Trigger Workflow**: Push a commit to trigger the workflow
3. **Branch Pattern**: Ensure workflow triggers on PR to main branch

### Status Checks Always Pending

1. **Workflow Syntax**: Check for YAML syntax errors in workflow file
2. **Job Dependencies**: Verify job dependencies are correctly configured
3. **GitHub Actions**: Check Actions tab for workflow execution logs

### Cannot Merge Despite Passing Checks

1. **Branch Up-to-Date**: Ensure feature branch is up-to-date with main
2. **All Checks**: Verify ALL required status checks have passed
3. **Review Required**: Ensure required reviews are approved

## 📊 Monitoring and Metrics

### Branch Protection Metrics

Track these metrics to measure effectiveness:

- **Direct Push Attempts**: Should be 0 (blocked by protection)
- **PR Merge Rate**: Percentage of PRs that pass all checks
- **Average PR Cycle Time**: Time from PR creation to merge
- **Failed Status Checks**: Number of PRs with failing checks

### Quality Metrics

Monitor code quality improvements:

- **Code Coverage Trend**: Should maintain or improve over time
- **Test Pass Rate**: Should be consistently high (>95%)
- **Security Vulnerabilities**: Should be 0 or quickly resolved
- **Code Review Feedback**: Track common review comments

## 🎯 Best Practices

### For Contributors

1. **Test Locally First**: Run all checks locally before pushing
2. **Small PRs**: Keep changes focused and reviewable
3. **Clear Descriptions**: Use PR template for consistent descriptions
4. **Respond to Feedback**: Address review comments promptly

### For Reviewers

1. **Timely Reviews**: Review PRs within 24 hours
2. **Constructive Feedback**: Provide specific, actionable feedback
3. **Event Sourcing Focus**: Pay special attention to ES patterns
4. **Security Mindset**: Consider security implications of changes

### For Maintainers

1. **Regular Updates**: Keep status checks and rules current
2. **Exception Handling**: Document any exceptions to protection rules
3. **Metrics Review**: Regularly review protection effectiveness
4. **Team Training**: Ensure team understands workflow and requirements

---

## 📋 Checklist for Setup

- [ ] Branch protection rule created for `main` branch
- [ ] All required status checks configured
- [ ] PR requirements set (approvals, up-to-date)
- [ ] Force push and deletion disabled
- [ ] CODEOWNERS file created (optional)
- [ ] Workflow file includes all required jobs
- [ ] Team members have appropriate permissions
- [ ] Protection rules tested with test PR
- [ ] Documentation updated with new workflow

---

This branch protection setup ensures that only high-quality, tested code makes it into the main branch while maintaining a smooth developer experience. 🚀 