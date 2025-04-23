function debugBusinessFilters() {
    const cards = document.querySelectorAll('.business-card');
    
    console.log("=== BUSINESS CARDS DATA ATTRIBUTES ===");
    cards.forEach((card, index) => {
        console.log(`Card ${index + 1}:`);
        console.log(`  Name: ${card.dataset.name}`);
        console.log(`  Category: ${card.dataset.category}`);
        console.log(`  Category ID: ${card.dataset.categoryId}`);
        console.log(`  Status: ${card.dataset.status}`);
        console.log(`  Package: ${card.dataset.package}`);
        console.log("-----");
    });
    
    const categoryFilter = document.getElementById('categoryFilter');
    const packageFilter = document.getElementById('packageFilter');
    const statusFilter = document.getElementById('statusFilter');
    
    console.log("=== FILTER OPTIONS ===");
    
    console.log("Category options:");
    if (categoryFilter) {
        Array.from(categoryFilter.options).forEach(option => {
            console.log(`  Value: ${option.value}, Text: ${option.text}`);
        });
    } else {
        console.log("  Category filter not found");
    }
    
    console.log("Package options:");
    if (packageFilter) {
        Array.from(packageFilter.options).forEach(option => {
            console.log(`  Value: ${option.value}, Text: ${option.text}`);
        });
    } else {
        console.log("  Package filter not found");
    }
    
    console.log("Status options:");
    if (statusFilter) {
        Array.from(statusFilter.options).forEach(option => {
            console.log(`  Value: ${option.value}, Text: ${option.text}`);
        });
    } else {
        console.log("  Status filter not found");
    }
    
    console.log("=== END DEBUG INFO ===");
    
    return "Debug info logged to console. Check browser developer tools.";
}

document.addEventListener('DOMContentLoaded', function() {
    console.log("Business filters debug script loaded");
    
    const debugBtn = document.createElement('button');
    debugBtn.textContent = "Debug Filters";
    debugBtn.className = "btn btn-secondary mt-3";
    debugBtn.style.position = "fixed";
    debugBtn.style.bottom = "20px";
    debugBtn.style.right = "20px";
    debugBtn.style.zIndex = "9999";
    debugBtn.onclick = function() {
        debugBusinessFilters();
        alert("Debug info logged to console. Press F12 to view.");
    };
    
    document.body.appendChild(debugBtn);
});
