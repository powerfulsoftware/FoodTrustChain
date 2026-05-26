// FoodTrustChain - Site JavaScript

document.addEventListener('DOMContentLoaded', function() {
    // Bootstrap alert auto-close
    setTimeout(function() {
        var alerts = document.querySelectorAll('.alert');
        alerts.forEach(function(alert) {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);

    // Confirm dialogs for delete actions
    document.querySelectorAll('a[href*="Sil"], a[href*="sil"]').forEach(function(link) {
        link.addEventListener('click', function(e) {
            if (!confirm('Bu işlemi yapmak istediğinizden emin misiniz?')) {
                e.preventDefault();
            }
        });
    });
});