namespace CardGeneratorBackend.DTO
{
    public record PaginationDTO<T>(IEnumerable<T> Data, int PageNumber, int PageSize, int TotalItemCount)
    {
        public PaginationDTO<E> Select<E>(Func<T, E> selectFunc)
        {
            var newData = Data.Select(selectFunc);
            return new(newData, PageNumber, PageSize, TotalItemCount);
        }
    }
}
