namespace ECommerce.Catalog.Test.DtoTests;

public class PagedResultTests
{
    [Fact]
    public void PagedResultAssignsPropertiesCorrectly()
    {
        // Arrange
        List<string> items = ["Item1", "Item2"];
        int totalCount = 10;
        int pageNumber = 1;
        int itemsPerPage = 2;
        int totalPages = 5;

        // Act
        PagedResult<string> result = new(items, totalCount, pageNumber, itemsPerPage, totalPages);

        // Assert
        Assert.Same(items, result.Items);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(itemsPerPage, result.ItemsPerPage);
        Assert.Equal(totalPages, result.TotalPages);
    }

    [Fact]
    public void PagedResultSupportsValueEquality()
    {
        // Arrange
        List<string> items = ["Item1", "Item2"];

        // Act
        PagedResult<string> result1 = new(items, 10, 1, 2, 5);
        PagedResult<string> result2 = new(items, 10, 1, 2, 5);

        // Assert
        // Because PagedResult is a 'record', equality is based on values, not memory references.
        Assert.Equal(result1, result2);
    }
}