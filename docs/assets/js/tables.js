function styleLastTdIfDescription() {
  const tables = document.querySelectorAll('.table-wrapper table');
  
  tables.forEach(table => {
    const headerRow = table.querySelector('thead tr');
    if (!headerRow) return;
    
    const lastTh = headerRow.querySelector('th:last-child');
    if (lastTh && lastTh.textContent.trim() === 'Description') {
        
        table.classList.add('has-description-header');
      const rows = table.querySelectorAll('tbody tr');

      rows.forEach(row => {
        const lastTd = row.querySelector('td:last-child');
        if (lastTd) {
            lastTd.classList.add('description-cell');
        }
      });
    }
  });
}

document.addEventListener('DOMContentLoaded', styleLastTdIfDescription);
document.addEventListener('docmd:navigate', styleLastTdIfDescription);