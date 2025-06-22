# Pull Request

## 📋 Description
<!-- Provide a brief description of the changes in this PR -->

## 🎯 Type of Change
<!-- Mark with an `x` all that apply -->
- [ ] 🐛 Bug fix (non-breaking change which fixes an issue)
- [ ] ✨ New feature (non-breaking change which adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] 📚 Documentation update
- [ ] 🧹 Code refactoring (no functional changes)
- [ ] ⚡ Performance improvement
- [ ] 🧪 Test improvements
- [ ] 🔧 Build/CI improvements

## 🔗 Related Issues
<!-- Link any related issues here -->
- Closes #(issue number)
- Relates to #(issue number)

## 🧪 Testing
<!-- Describe the tests you ran to verify your changes -->

### Test Coverage
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed
- [ ] Coverage threshold maintained (≥60%)

### Test Results
```
# Paste test results here
```

## 📝 Checklist
<!-- Mark with an `x` all that apply -->

### Code Quality
- [ ] Code follows the project's coding standards
- [ ] Self-review of code completed
- [ ] Code is properly commented (especially complex logic)
- [ ] No debugging code or console.log statements left
- [ ] All compiler warnings resolved

### Documentation
- [ ] XML documentation added for public APIs
- [ ] README updated (if applicable)
- [ ] API documentation updated (if applicable)
- [ ] Architecture diagrams updated (if applicable)

### Security & Performance
- [ ] No sensitive information exposed
- [ ] Performance impact considered
- [ ] Security implications reviewed
- [ ] Dependencies updated (if applicable)

### Event Sourcing Specific
- [ ] Events are immutable and properly versioned
- [ ] Projection handlers updated (if new events added)
- [ ] Migration strategy considered for event schema changes
- [ ] Aggregate invariants maintained

## 🖼️ Screenshots/Demos
<!-- Add screenshots or demo videos if applicable -->

## 🚀 Deployment Notes
<!-- Any special deployment considerations -->

## 📊 Performance Impact
<!-- Describe any performance implications -->

## 🔄 Migration Notes
<!-- Any database migrations or breaking changes -->

---

## 📋 Reviewer Guidelines

### What to Look For:
1. **Event Sourcing Patterns**: Proper use of events, aggregates, and projections
2. **CQRS Implementation**: Clear separation of commands and queries
3. **Code Quality**: Readability, maintainability, and adherence to patterns
4. **Test Coverage**: Adequate unit and integration test coverage
5. **Documentation**: API documentation and code comments
6. **Security**: No sensitive data exposure, proper validation
7. **Performance**: Efficient queries and event handling

### Testing Checklist:
- [ ] Pull branch and run locally
- [ ] Execute full test suite
- [ ] Test API endpoints (if applicable)
- [ ] Verify event sourcing behavior
- [ ] Check read model consistency
- [ ] Validate time travel queries (if applicable) 