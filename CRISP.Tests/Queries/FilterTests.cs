namespace CRISP.Tests.Queries
{
    public class FilterTests
    {
        [Fact]
        public void SearchFilter_Constructor_InitializesProperties()
        {
            // Act
            SearchFilter filter = new();

            // Assert
            filter.ShouldNotBeNull();
            filter.Search.ShouldBeNull();
            filter.Page.ShouldBe(1); // Default value
            filter.PageSize.ShouldBe(int.MaxValue); // Default value
        }

        [Fact]
        public void SearchFilter_SetSearchTerm_UpdatesProperty()
        {
            // Arrange
            SearchFilter filter = new()
            {
                // Act
                Search = "test query"
            };

            // Assert
            filter.Search.ShouldBe("test query");
        }

        [Fact]
        public void DateRangeFilter_Constructor_InitializesProperties()
        {
            // Act
            DateRangeFilter filter = new();

            // Assert
            filter.ShouldNotBeNull();
            filter.StartDate.ShouldBe(null);
            filter.EndDate.ShouldBe(null);
        }

        [Fact]
        public void DateRangeFilter_SetProperties_UpdatesValues()
        {
            // Arrange
            DateRangeFilter filter = new();
            DateTime start = DateTime.Now.AddDays(-7);
            DateTime end = DateTime.Now;

            // Act
            filter.StartDate = start;
            filter.EndDate = end;

            // Assert
            filter.StartDate.ShouldBe(start);
            filter.EndDate.ShouldBe(end);
        }

        [Fact]
        public void FilterBase_SetPage_Updaterty()
        {
            // Arrange
            TestFilter filter = new()
            {
                // Act
                Page = 5
            };

            // Assert
            filter.Page.ShouldBe(5);
        }

        [Fact]
        public void FilterBase_SetPageSize_UpdatesProperty()
        {
            // Arrange
            TestFilter filter = new()
            {
                // Act
                PageSize = 20
            };

            // Assert
            filter.PageSize.ShouldBe(20);
        }

        [Fact]
        public void FilterBase_SetSortBy_UpdatesProperty()
        {
            // Arrange
            TestFilter filter = new()
            {
                // Act
                SortBy = "Name"
            };

            // Assert
            filter.SortBy.ShouldBe("Name");
        }

        [Fact]
        public void FilterBase_SetSortDescending_UpdatesProperty()
        {
            // Arrange
            TestFilter filter = new()
            {
                // Act
                SortDescending = true
            };

            // Assert
            filter.SortDescending.ShouldBeTrue();
        }

        // Helper class for testing FilterBase
        private class TestFilter : FilterBase
        {
        }
    }
}