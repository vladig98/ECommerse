namespace ECommerce.Catalog.Dtos;

public record class PagedResult<T>
(
    List<T> Items, 
    int TotalCount, 
    int PageNumber, 
    int ItemsPerPage, 
    int TotalPages
);
