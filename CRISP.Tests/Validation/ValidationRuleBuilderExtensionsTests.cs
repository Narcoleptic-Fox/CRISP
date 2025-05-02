using CRISP.Validation;

namespace CRISP.Tests.Validation
{
    public class ValidationRuleBuilderExtensionsTests
    {
        private class TestModel
        {
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
            public DateTime DateProperty { get; set; }
            public List<string> ListProperty { get; set; } = [];
            public TestNestedModel NestedProperty { get; set; }
            public List<TestNestedModel> NestedListProperty { get; set; } = [];
            public string ZipCode { get; set; }
            public string Email { get; set; }
        }

        private class TestNestedModel
        {
            public string NestedStringProperty { get; set; }
        }

        private class TestValidator : FluentValidator<TestModel>
        {
            protected override void ConfigureValidationRules()
            {
                // Rules will be configured in tests
            }

            // Expose RuleFor for testing
            public IRuleBuilder<TestModel, TProperty> TestRuleFor<TProperty>(System.Linq.Expressions.Expression<Func<TestModel, TProperty>> expression) => RuleFor(expression);
        }

        [Fact]
        public void NotEmpty_RejectsNullAndEmptyStrings()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, string> rule = validator.TestRuleFor(m => m.StringProperty).NotEmpty();

            TestModel model = new() { StringProperty = null };
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "StringProperty");

            // Test with empty string
            model.StringProperty = "";
            result = validator.Validate(model);
            result.IsValid.ShouldBeFalse();

            // Test with whitespace
            model.StringProperty = "   ";
            result = validator.Validate(model);
            result.IsValid.ShouldBeFalse();

            // Test with valid value
            model.StringProperty = "Valid";
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void NotNull_RejectsNullValues()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, TestNestedModel?> rule = validator.TestRuleFor(m => m.NestedProperty).NotNull();

            TestModel model = new() { NestedProperty = null };
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "NestedProperty");

            // Test with valid value
            model.NestedProperty = new TestNestedModel();
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void MinCount_RejectsEmptyCollections()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, List<string>> rule = validator.TestRuleFor(m => m.ListProperty).MinCount(2);

            TestModel model = new() { ListProperty = [] };
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "ListProperty");

            // Test with too few items
            model.ListProperty = ["Item1"];
            result = validator.Validate(model);
            result.IsValid.ShouldBeFalse();

            // Test with exactly minimum
            model.ListProperty = ["Item1", "Item2"];
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();

            // Test with more than minimum
            model.ListProperty = ["Item1", "Item2", "Item3"];
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void MaxCount_RejectsOverflowCollections()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, List<string>> rule = validator.TestRuleFor(m => m.ListProperty).MaxCount(2);

            TestModel model = new() { ListProperty = ["Item1", "Item2", "Item3"] };
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "ListProperty");

            // Test with exactly maximum
            model.ListProperty = ["Item1", "Item2"];
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();

            // Test with fewer than maximum
            model.ListProperty = ["Item1"];
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();

            // Test with empty collection
            model.ListProperty = [];
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void ForEach_ValidatesEachItemInCollection()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, List<string>> rule = validator.TestRuleFor(m => m.ListProperty)
                .ForEach(item => !string.IsNullOrEmpty(item) && item.Length > 3, "Each item must be at least 3 characters");

            TestModel model = new() { ListProperty = ["abc", "a", "abcd"] };
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "ListProperty");

            // Test with all valid items
            model.ListProperty = ["abcd", "abcde"];
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Length_ValidatesStringLength()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, string> rule = validator.TestRuleFor(m => m.StringProperty).Length(3, 5);

            TestModel model = new() { StringProperty = "ab" }; // Too short
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "StringProperty");

            // Test with too long
            model.StringProperty = "abcdef"; // Too long
            result = validator.Validate(model);
            result.IsValid.ShouldBeFalse();

            // Test with minimum length
            model.StringProperty = "abc"; // Exactly min
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();

            // Test with maximum length
            model.StringProperty = "abcde"; // Exactly max
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void MinLength_ValidatesMinimumStringLength()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, string> rule = validator.TestRuleFor(m => m.StringProperty).MinLength(3);

            TestModel model = new() { StringProperty = "ab" }; // Too short
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "StringProperty");

            // Test with minimum length
            model.StringProperty = "abc"; // Exactly min
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();

            // Test with longer than minimum
            model.StringProperty = "abcdef";
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void MaxLength_ValidatesMaximumStringLength()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, string> rule = validator.TestRuleFor(m => m.StringProperty).MaxLength(5);

            TestModel model = new() { StringProperty = "abcdef" }; // Too long
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "StringProperty");

            // Test with maximum length
            model.StringProperty = "abcde"; // Exactly max
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();

            // Test with shorter than maximum
            model.StringProperty = "abc";
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();

            // Test with null value (should pass)
            model.StringProperty = null;
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Matches_ValidatesRegexPattern()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, string> rule = validator.TestRuleFor(m => m.ZipCode).Matches(@"^\d{5}(-\d{4})?$");

            TestModel model = new() { ZipCode = "invalid" }; // Not a valid ZIP
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "ZipCode");

            // Test with valid 5-digit ZIP
            model.ZipCode = "12345";
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();

            // Test with valid 9-digit ZIP
            model.ZipCode = "12345-6789";
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void EmailAddress_ValidatesEmailFormat()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, string> rule = validator.TestRuleFor(m => m.Email).EmailAddress();

            TestModel model = new() { Email = "not-an-email" }; // Not a valid email
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Email");

            // Test with valid email
            model.Email = "test@example.com";
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();

            // Test with complex but valid email
            model.Email = "user.name+tag@example.co.uk";
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void GreaterThan_ValidatesNumericComparison()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, int> rule = validator.TestRuleFor(m => m.IntProperty).GreaterThan(10);

            TestModel model = new() { IntProperty = 5 }; // Less than threshold
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "IntProperty");

            // Test with equal value (should fail)
            model.IntProperty = 10;
            result = validator.Validate(model);
            result.IsValid.ShouldBeFalse();

            // Test with greater value
            model.IntProperty = 11;
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void LessThan_ValidatesNumericComparison()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, int> rule = validator.TestRuleFor(m => m.IntProperty).LessThan(10);

            TestModel model = new() { IntProperty = 15 }; // Greater than threshold
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "IntProperty");

            // Test with equal value (should fail)
            model.IntProperty = 10;
            result = validator.Validate(model);
            result.IsValid.ShouldBeFalse();

            // Test with less value
            model.IntProperty = 9;
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Must_CustomValidationRule()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, int> rule = validator.TestRuleFor(m => m.IntProperty)
                .Must(value => value % 2 == 0, "Value must be even");

            TestModel model = new() { IntProperty = 5 }; // Odd number
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "IntProperty" && e.ErrorMessage == "Value must be even");

            // Test with even number
            model.IntProperty = 6;
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void In_ValidatesValueInSet()
        {
            // Arrange
            TestValidator validator = new();
            IRuleBuilder<TestModel, int> rule = validator.TestRuleFor(m => m.IntProperty).In(1, 3, 5, 7, 9);

            TestModel model = new() { IntProperty = 2 }; // Not in the set
            ValidationResult result = validator.Validate(model);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "IntProperty");

            // Test with value in set
            model.IntProperty = 5;
            result = validator.Validate(model);
            result.IsValid.ShouldBeTrue();
        }
    }
}