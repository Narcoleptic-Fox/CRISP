namespace CRISP.Responses
{
    public sealed record FilteredResponse<T>
        where T : class
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int TotalItems { get; set; }
    }
}
