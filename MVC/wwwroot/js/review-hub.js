/**
 * Library for managing and displaying reviews interactively using SignalR
 */
document.addEventListener('DOMContentLoaded', function() {
    // Wait for SignalR to load
    const initReviewHub = () => {
        // Check if SignalR library is available
        if (typeof signalR === 'undefined') {
            console.error('SignalR is not loaded yet');
            
            // Listen for SignalR loaded event
            document.addEventListener('signalRLoaded', initReviewHub);
            return;
        }
        
        // Initialize connection with SignalR
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/reviewhub")
            .withAutomaticReconnect()
            .build();
            
        // Event on reconnecting
        connection.onreconnecting(error => {
            console.warn('Connection lost. Attempting to reconnect...', error);
            showReviewStatus('Reconnecting...');
        });
        
        connection.onreconnected(connectionId => {
            console.log('Reconnected. ID:', connectionId);
            hideReviewStatus();
            loadLatestReviews(); // Update reviews
        });

        // Start connection
        connection.start()
            .then(() => {
                console.log("Connected to review hub");
                hideReviewStatus();
                setupReviewForm();
                loadLatestReviews();
            })
            .catch(err => {
                console.error("Error connecting to review hub:", err);
                showReviewStatus('Failed to connect to the server - reviews will not update automatically');
            });
        
        // Show connection status
        function showReviewStatus(message) {
            let statusEl = document.getElementById('review-connection-status');
            
            if (!statusEl && document.getElementById('reviewsContainer')) {
                statusEl = document.createElement('div');
                statusEl.id = 'review-connection-status';
                statusEl.className = 'alert alert-warning m-3';
                document.getElementById('reviewsContainer').prepend(statusEl);
            }
            
            if (statusEl) {
                statusEl.innerHTML = `
                    <i class="fas fa-exclamation-triangle me-2"></i>${message}
                `;
                
                statusEl.style.display = 'block';
            }
        }
        
        // Hide connection status
        function hideReviewStatus() {
            const statusEl = document.getElementById('review-connection-status');
            if (statusEl) {
                statusEl.style.display = 'none';
            }
        }
        
        // Load latest reviews
        function loadLatestReviews() {
            const reviewsContainer = document.getElementById('reviewsContainer');
            const businessId = document.getElementById('businessId')?.value;
            
            if (!reviewsContainer || !businessId) return;
            
            // Display loading message
            reviewsContainer.innerHTML = `
                <div class="text-center py-4">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="mt-2">Loading reviews...</p>
                </div>
            `;
            
            connection.invoke("GetBusinessReviews", businessId)
                .then(reviews => {
                    renderReviews(reviews);
                })
                .catch(err => {
                    console.error("Error loading reviews:", err);
                    reviewsContainer.innerHTML = `
                        <div class="alert alert-danger m-3">
                            <i class="fas fa-exclamation-circle me-2"></i>
                            Failed to load reviews. Please try again.
                        </div>
                    `;
                });
        }
        
        // Render reviews
        function renderReviews(reviews) {
            const reviewsContainer = document.getElementById('reviewsContainer');
            if (!reviewsContainer) return;
            
            if (reviews.length === 0) {
                reviewsContainer.innerHTML = `
                    <div class="alert alert-info m-3">
                        <i class="fas fa-info-circle me-2"></i>
                        No reviews for this business yet. Be the first to leave a review!
                    </div>
                `;
                return;
            }
            
            reviewsContainer.innerHTML = '';
            
            // Sort reviews from newest to oldest
            reviews.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
            
            reviews.forEach(review => {
                const reviewCard = document.createElement('div');
                reviewCard.className = 'card mb-3 review-card';
                reviewCard.dataset.reviewId = review.id;
                
                const createdDate = new Date(review.createdAt);
                const formattedDate = createdDate.toLocaleDateString('en-US', { 
                    year: 'numeric', 
                    month: 'long', 
                    day: 'numeric' 
                });
                
                // Build stars display
                let starsHtml = '';
                for (let i = 1; i <= 5; i++) {
                    if (i <= review.rating) {
                        starsHtml += '<i class="fas fa-star text-warning"></i>';
                    } else {
                        starsHtml += '<i class="far fa-star text-warning"></i>';
                    }
                }
                
                // First letter of user's email
                const userInitial = review.email.charAt(0).toUpperCase();
                
                reviewCard.innerHTML = `
                    <div class="card-body">
                        <div class="d-flex align-items-center mb-3">
                            <div class="review-avatar">${userInitial}</div>
                            <div class="ms-3">
                                <div class="review-stars">
                                    ${starsHtml}
                                </div>
                                <h6 class="mb-0">${maskEmail(review.email)}</h6>
                                <small class="text-muted">${formattedDate}</small>
                            </div>
                        </div>
                        <p class="card-text">${escapeHtml(review.comment)}</p>
                    </div>
                `;
                
                reviewsContainer.appendChild(reviewCard);
            });
            
            // Update review count and average
            updateReviewSummary(reviews);
        }
        
        // Set up review form
        function setupReviewForm() {
            const reviewForm = document.getElementById('reviewForm');
            
            if (!reviewForm) return;
            
            reviewForm.addEventListener('submit', function(e) {
                e.preventDefault();
                
                const businessId = document.getElementById('businessId').value;
                const rating = document.querySelector('input[name="rating"]:checked')?.value;
                const email = document.getElementById('email').value;
                const comment = document.getElementById('comment').value;
                
                // Validate data
                if (!rating) {
                    showFormError('Please select a star rating');
                    return;
                }
                
                if (!email || !isValidEmail(email)) {
                    showFormError('Please enter a valid email');
                    return;
                }
                
                if (!comment) {
                    showFormError('Please write a comment');
                    return;
                }
                
                // Show submission status
                const submitBtn = reviewForm.querySelector('button[type="submit"]');
                const originalText = submitBtn.innerHTML;
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Submitting...';
                
                // Send review
                connection.invoke("SubmitReview", {
                    businessId,
                    rating: parseInt(rating),
                    email,
                    comment
                })
                .then(() => {
                    // Show success message
                    const successAlert = document.createElement('div');
                    successAlert.className = 'alert alert-success alert-dismissible fade show mt-3';
                    successAlert.innerHTML = `
                        <i class="fas fa-check-circle me-2"></i>
                        Your review was submitted successfully. Thank you!
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                    `;
                    reviewForm.appendChild(successAlert);
                    
                    // Reset form
                    reviewForm.reset();
                    document.querySelectorAll('.rating-star').forEach(star => {
                        star.classList.remove('selected');
                    });
                    
                    // Hide success message after 5 seconds
                    setTimeout(() => {
                        successAlert.remove();
                    }, 5000);
                })
                .catch(err => {
                    console.error("Error submitting review:", err);
                    showFormError('An error occurred while submitting your review. Please try again.');
                })
                .finally(() => {
                    // Re-enable submit button
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                });
            });
            
            // Star rating system
            const ratingStars = document.querySelectorAll('.rating-star');
            ratingStars.forEach(star => {
                star.addEventListener('click', function() {
                    const rating = this.dataset.rating;
                    document.getElementById('ratingValue').value = rating;
                    
                    // Update star appearance
                    ratingStars.forEach(s => {
                        if (s.dataset.rating <= rating) {
                            s.classList.add('selected');
                        } else {
                            s.classList.remove('selected');
                        }
                    });
                });
            });
        }
        
        // Show form error
        function showFormError(message) {
            let errorElement = document.getElementById('reviewFormError');
            
            if (!errorElement) {
                errorElement = document.createElement('div');
                errorElement.id = 'reviewFormError';
                errorElement.className = 'alert alert-danger mt-3';
                document.getElementById('reviewForm').prepend(errorElement);
            }
            
            errorElement.innerHTML = `<i class="fas fa-exclamation-circle me-2"></i>${message}`;
            
            // Scroll to error
            errorElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
            
            // Hide error after 5 seconds
            setTimeout(() => {
                errorElement.remove();
            }, 5000);
        }
        
        // Update review summary
        function updateReviewSummary(reviews) {
            const totalReviews = reviews.length;
            const avgRating = reviews.reduce((sum, review) => sum + review.rating, 0) / totalReviews;
            
            const ratingCountElement = document.getElementById('ratingCount');
            const avgRatingElement = document.getElementById('avgRating');
            const ratingStarsElement = document.getElementById('ratingStars');
            
            if (ratingCountElement) {
                ratingCountElement.textContent = totalReviews;
            }
            
            if (avgRatingElement) {
                avgRatingElement.textContent = avgRating.toFixed(1);
            }
            
            if (ratingStarsElement) {
                let starsHtml = '';
                for (let i = 1; i <= 5; i++) {
                    if (i <= Math.round(avgRating)) {
                        starsHtml += '<i class="fas fa-star text-warning"></i>';
                    } else if (i - 0.5 <= avgRating) {
                        starsHtml += '<i class="fas fa-star-half-alt text-warning"></i>';
                    } else {
                        starsHtml += '<i class="far fa-star text-warning"></i>';
                    }
                }
                ratingStarsElement.innerHTML = starsHtml;
            }
        }
        
        // Receive new reviews (live broadcast)
        connection.on("NewReviewAdded", function(businessId) {
            // Update reviews only if we're on the same business page
            const currentBusinessId = document.getElementById('businessId')?.value;
            
            if (currentBusinessId && currentBusinessId == businessId) {
                // Short delay to ensure the review was saved in the database
                setTimeout(() => {
                    loadLatestReviews();
                }, 500);
            }
        });
        
        // Helper functions
        function isValidEmail(email) {
            const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return re.test(String(email).toLowerCase());
        }
        
        function escapeHtml(text) {
            if (!text) return '';
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }
        
        function maskEmail(email) {
            if (!email) return '';
            const parts = email.split('@');
            if (parts.length !== 2) return email;
            
            const username = parts[0];
            const domain = parts[1];
            
            let maskedUsername;
            if (username.length <= 2) {
                maskedUsername = username;
            } else {
                maskedUsername = username.substring(0, 1) + 
                                '***' + 
                                username.substring(username.length - 1);
            }
            
            return maskedUsername + '@' + domain;
        }
    };

    // Start initialization
    initReviewHub();
});
