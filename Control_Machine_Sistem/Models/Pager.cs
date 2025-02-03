namespace Control_Machine_Sistem.Models
{
    public class Pager<T>
    {
        public List<T> Elements { get; private set; }
        public int TotalElements { get; private set; }
        public int ElementsPerPage { get; private set; }
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }

        public Pager(List<T> elements, int totalElements, int currentPage, int elementsPerPage)
        {
            Elements = elements;
            TotalElements = totalElements;
            CurrentPage = currentPage;
            ElementsPerPage = elementsPerPage;
            TotalPages = (int)Math.Ceiling(totalElements / (double)elementsPerPage);
        }

        public bool PreviousPage => CurrentPage > 1;
        public bool NextPage => CurrentPage < TotalPages;
    }

}
