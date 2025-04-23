// myBusiness page functionality
document.addEventListener('DOMContentLoaded', function() {
    // Variables
    const searchInput = document.getElementById('businessSearch');
    const statusFilter = document.getElementById('statusFilter');
    const packageFilter = document.getElementById('packageFilter');
    const categoryFilter = document.getElementById('categoryFilter');
    const businessGrid = document.getElementById('businessGrid');
    const businessCards = document.querySelectorAll('.business-card');
    const pagination = document.getElementById('businessPagination');
    const paginationLoader = document.getElementById('paginationLoader');
    const loadingOverlay = document.getElementById('loadingOverlay');
    
    // Initialize Bootstrap components
    initializeBootstrapComponents();
    
    // Filter businesses based on search and filter criteria
    function filterBusinesses() {
        const searchTerm = searchInput?.value.toLowerCase() || '';
        const statusValue = statusFilter?.value || 'all';
        const packageValue = packageFilter?.value || 'all';
        const categoryValue = categoryFilter?.value || 'all';
        
        let visibleCount = 0;
        
        businessCards.forEach(card => {
            const nameMatch = card.dataset.name.includes(searchTerm);
            const categoryMatch = card.dataset.category.includes(searchTerm);
            const statusMatch = statusValue === 'all' || card.dataset.status === statusValue;
            const packageMatch = packageValue === 'all' || (packageValue.toLowerCase() === card.dataset.package.toLowerCase());
            const categoryIdMatch = categoryValue === 'all' || card.dataset.categoryId === categoryValue;
            
            if ((nameMatch || categoryMatch) && statusMatch && packageMatch && categoryIdMatch) {
                card.style.display = '';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });
        
        handleNoResults(visibleCount);
    }
    
    function handleNoResults(visibleCount) {
        let noResultsElement = businessGrid.querySelector('.no-results');
        
        if (visibleCount === 0) {
            if (!noResultsElement) {
                noResultsElement = document.createElement('div');
                noResultsElement.className = 'no-results';
                noResultsElement.innerHTML = `
                    <i class="fas fa-search"></i>
                    <h3>No matching businesses found</h3>
                    <p>Try adjusting your search criteria or filters.</p>
                `;
                businessGrid.appendChild(noResultsElement);
            }
        } else if (noResultsElement) {
            noResultsElement.remove();
        }
    }
    
    // Event listeners for real-time filtering
    if (searchInput) {
        searchInput.addEventListener('input', filterBusinesses);
    }
    
    if (statusFilter) {
        statusFilter.addEventListener('change', filterBusinesses);
    }
    
    if (packageFilter) {
        packageFilter.addEventListener('change', filterBusinesses);
    }
    
    if (categoryFilter) {
        categoryFilter.addEventListener('change', filterBusinesses);
    }
    
    // Pagination functionality
    if (pagination) {
        pagination.addEventListener('click', function(e) {
            const pageLink = e.target.closest('.page-link');
            if (pageLink && !pageLink.parentElement.classList.contains('disabled')) {
                e.preventDefault();
                const pageNumber = pageLink.getAttribute('data-page');
                loadBusinessPage(pageNumber);
            }
        });
    }
    
    function loadBusinessPage(page) {
        if (paginationLoader) paginationLoader.classList.add('active');
        
        businessCards.forEach(card => {
            card.style.opacity = '0.6';
        });
        
        const searchTerm = searchInput ? searchInput.value : '';
        const statusValue = statusFilter ? statusFilter.value : 'all';
        const packageValue = packageFilter ? packageFilter.value : 'all';
        const categoryValue = categoryFilter ? categoryFilter.value : 'all';
        
        // Fetch data with appropriate query parameters
        const userId = document.querySelector('input[name="userId"]')?.value;
        fetch(`/Business/GetBusinessByUserIdPaged?id=${userId}&page=${page}&searchTerm=${encodeURIComponent(searchTerm)}&status=${statusValue}&package=${packageValue}&category=${categoryValue}`, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => response.text())
        .then(html => {
            setTimeout(() => {
                businessGrid.innerHTML = html;
                updatePaginationUI(page);
                
                if (paginationLoader) paginationLoader.classList.remove('active');
                
                document.querySelectorAll('.business-card').forEach(card => {
                    card.style.opacity = '1';
                });
                
                businessGrid.scrollIntoView({ behavior: 'smooth', block: 'start' });
                bindFilterListeners();
            }, 500);
        })
        .catch(error => {
            console.error('Error loading businesses:', error);
            if (paginationLoader) paginationLoader.classList.remove('active');
            
            businessCards.forEach(card => {
                card.style.opacity = '1';
            });
        });
    }
    
    function bindFilterListeners() {
        // Rebind events after content replacement
        initializeBootstrapComponents();
        filterBusinesses();
    }
    
    function updatePaginationUI(currentPage) {
        currentPage = parseInt(currentPage);
        const totalPages = parseInt(document.querySelector('input[name="totalPages"]')?.value || '1');
        
        if (!pagination) return;
        
        document.querySelectorAll('#businessPagination .page-item').forEach(item => {
            item.classList.remove('active');
            
            const pageLink = item.querySelector('.page-link');
            if (pageLink && pageLink.getAttribute('data-page') == currentPage) {
                item.classList.add('active');
            }
        });
        
        const prevButton = document.querySelector('#businessPagination .page-item:first-child');
        const nextButton = document.querySelector('#businessPagination .page-item:last-child');
        
        if (prevButton) {
            if (currentPage <= 1) {
                prevButton.classList.add('disabled');
            } else {
                prevButton.classList.remove('disabled');
                prevButton.querySelector('.page-link').setAttribute('data-page', currentPage - 1);
            }
        }
        
        if (nextButton) {
            if (currentPage >= totalPages) {
                nextButton.classList.add('disabled');
            } else {
                nextButton.classList.remove('disabled');
                nextButton.querySelector('.page-link').setAttribute('data-page', currentPage + 1);
            }
        }
    }
    
    // Initialize Bootstrap components including modal
    function initializeBootstrapComponents() {
        window.confirmDelete = function(id, name) {
            // Set content for delete modal
            const nameElement = document.getElementById('businessNameToDelete');
            const confirmBtn = document.getElementById('confirmDeleteBtn');
            
            if (nameElement) nameElement.textContent = name;
            if (confirmBtn) confirmBtn.href = '/Business/Delete?id=' + id;
            
            // Initialize and show modal
            const deleteModalElement = document.getElementById('deleteModal');
            if (deleteModalElement) {
                const deleteModal = new bootstrap.Modal(deleteModalElement);
                deleteModal.show();
            }
        };
    }
    
    // Handle success/error messages
    function showNotification(type, message) {
        const container = document.createElement('div');
        container.className = 'position-fixed top-0 end-0 p-3';
        container.style.zIndex = '1050';
        
        const toastElement = document.createElement('div');
        toastElement.className = `toast align-items-center text-white bg-${type === 'success' ? 'success' : 'danger'}`;
        toastElement.setAttribute('role', 'alert');
        toastElement.setAttribute('aria-live', 'assertive');
        toastElement.setAttribute('aria-atomic', 'true');
        
        toastElement.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;
        
        container.appendChild(toastElement);
        document.body.appendChild(container);
        
        const toast = new bootstrap.Toast(toastElement, { delay: 5000 });
        toast.show();
        
        // Remove after hiding
        toastElement.addEventListener('hidden.bs.toast', function() {
            container.remove();
        });
    }
    
    // Check for TempData messages
    const successMessage = document.querySelector('input[name="successMessage"]')?.value;
    const errorMessage = document.querySelector('input[name="errorMessage"]')?.value;
    
    if (successMessage) {
        showNotification('success', successMessage);
    }
    
    if (errorMessage) {
        showNotification('error', errorMessage);
    }
});
