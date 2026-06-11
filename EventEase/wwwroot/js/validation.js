// Real-time availability check
document.addEventListener('DOMContentLoaded', function () {
    const venueSelect = document.getElementById('VenueId');
    const bookingDateInput = document.getElementById('BookingDate');
    const availabilityMessage = document.getElementById('availabilityMessage');

    if (venueSelect && bookingDateInput) {
        async function checkAvailability() {
            const venueId = venueSelect.value;
            const bookingDate = bookingDateInput.value;

            if (venueId && bookingDate) {
                availabilityMessage.style.display = 'block';
                availabilityMessage.className = 'alert alert-info';
                availabilityMessage.textContent = 'Checking availability...';

                try {
                    const response = await fetch(`/Bookings/CheckAvailability?venueId=${venueId}&date=${bookingDate}`);
                    const data = await response.json();

                    if (data.isAvailable) {
                        availabilityMessage.className = 'alert alert-success';
                        availabilityMessage.textContent = '✓ Venue is available on this date!';
                    } else {
                        availabilityMessage.className = 'alert alert-danger';
                        availabilityMessage.textContent = '✗ This venue is already booked on this date.';
                        document.getElementById('submitBtn').disabled = true;
                        return;
                    }
                } catch (error) {
                    availabilityMessage.className = 'alert alert-warning';
                    availabilityMessage.textContent = 'Unable to check availability. Please submit and the system will validate.';
                }
                document.getElementById('submitBtn').disabled = false;
            } else {
                availabilityMessage.style.display = 'none';
            }
        }

        venueSelect.addEventListener('change', checkAvailability);
        bookingDateInput.addEventListener('change', checkAvailability);
    }

    // Form validation on submit
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
                showAlert('Please fill in all required fields correctly.', 'danger');
            }
            form.classList.add('was-validated');
        });
    });
});

function showAlert(message, type) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.role = 'alert';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.querySelector('.container').prepend(alertDiv);
    setTimeout(() => alertDiv.remove(), 5000);
}