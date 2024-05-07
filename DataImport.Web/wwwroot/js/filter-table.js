document.addEventListener('DOMContentLoaded', function () {
    const select = document.getElementById('filterValues');
    const table = document.querySelector('table');
    const rows = table.getElementsByTagName('tr');

    select.addEventListener('change', function () {
        const selectedValue = this.value.toLowerCase();
        // Find the index of the column with the class "filterable"
        let columnIndex = -1;
        const headers = table.querySelectorAll('th.filterable');
        headers.forEach((th, index) => {
            columnIndex = th.cellIndex;
        });

        for (let i = 0; i < rows.length; i++) {
            const cell = rows[i].getElementsByTagName('td')[columnIndex];
            if (cell) {
                const cellValue = cell.textContent.trim().toLowerCase();

                if (selectedValue === 'all' || cellValue === selectedValue) {
                    // Show the row
                    rows[i].style.display = '';
                } else {
                    // Hide the row
                    rows[i].style.display = 'none';
                }
            }
        }
    });
});
