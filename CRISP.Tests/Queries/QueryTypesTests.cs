namespace CRISP.Tests.Queries
{
    public class QueryTypesTests
    {
        [Fact]
        public void ByIdQuery_Constructor_SetsIdProperty()
        {
            // Arrange
            Guid id = Guid.NewGuid();

            // Act
            TestByIdQuery query = new() { Id = id };

            // Assert
            query.Id.ShouldBe(id);
        }

        [Fact]
        public void ByIdQuery_InheritsFromQuery()
        {
            // Arrange & Act
            TestByIdQuery query = new();

            // Assert
            query.ShouldBeAssignableTo<Query<TestResponse>>();
            query.ShouldBeAssignableTo<IRequest<Response<TestResponse>>>();
        }

        [Fact]
        public void FilteredQuery_Constructor_SetsFilterProperty()
        {
            // Arrange
            TestFilter filter = new() { SearchTerm = "test" };

            // Act
            TestFilteredQuery query = new() { Filter = filter };

            // Assert
            query.Filter.ShouldBe(filter);
            query.Filter.SearchTerm.ShouldBe("test");
        }

        [Fact]
        public void FilteredQuery_InheritsFromQuery()
        {
            // Arrange & Act
            TestFilteredQuery query = new();

            // Assert
            query.ShouldBeAssignableTo<Query<PagedResult<TestResponse>>>();
            query.ShouldBeAssignableTo<IRequest<Response<PagedResult<TestResponse>>>>();
        }

        // Helper classes for testing
        private class TestResponse { }

        private class TestFilter : FilterBase
        {
            public string? SearchTerm { get; set; }
        }

        private record TestByIdQuery : ByIdQuery<Guid, TestResponse>
        {
        }

        private record TestFilteredQuery : FilteredQuery<TestFilter, TestResponse>
        {
        }
    }
}