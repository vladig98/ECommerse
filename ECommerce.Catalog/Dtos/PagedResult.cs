namespace ECommerce.Catalog.Dtos;

internal record class PagedResult<T>
(
    List<T> Items, 
    int TotalCount, 
    int PageNumber, 
    int ItemsPerPage, 
    int TotalPages
);
