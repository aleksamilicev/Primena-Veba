import "../../styles/Pagination.css"; // dodajte ovaj CSS fajl

export default function Pagination({ pagination, onPageChange }) {
  if (!pagination || pagination.totalPages <= 1) return null;

  const { currentPage, totalPages } = pagination;
  
  // Generiši page numbers za prikaz
  const getPageNumbers = () => {
    const pages = [];
    const showPages = 5; // Broj stranica za prikaz
    
    if (totalPages <= showPages) {
      // Ako imamo malo stranica, prikaži sve
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Logika za veći broj stranica
      const start = Math.max(1, currentPage - 2);
      const end = Math.min(totalPages, currentPage + 2);
      
      if (start > 1) {
        pages.push(1);
        if (start > 2) pages.push('...');
      }
      
      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
      
      if (end < totalPages) {
        if (end < totalPages - 1) pages.push('...');
        pages.push(totalPages);
      }
    }
    
    return pages;
  };

  const pageNumbers = getPageNumbers();

  return (
    <div className="pagination-container">
      <div className="pagination">
        {/* Previous button */}
        <button
          disabled={currentPage === 1}
          onClick={() => onPageChange(currentPage - 1)}
          className="pagination-btn prev-btn"
          title="Prethodna stranica"
        >
          ←
        </button>

        {/* Page numbers */}
        <div className="page-numbers">
          {pageNumbers.map((page, index) => (
            <button
              key={index}
              disabled={page === '...'}
              onClick={() => typeof page === 'number' ? onPageChange(page) : null}
              className={`page-btn ${
                page === currentPage ? 'active' : ''
              } ${page === '...' ? 'dots' : ''}`}
            >
              {page}
            </button>
          ))}
        </div>

        {/* Next button */}
        <button
          disabled={currentPage === totalPages}
          onClick={() => onPageChange(currentPage + 1)}
          className="pagination-btn next-btn"
          title="Sledeća stranica"
        >
          →
        </button>
      </div>
      
      {/* Page info */}
      <div className="page-info">
        Stranica {currentPage} od {totalPages}
      </div>
    </div>
  );
}