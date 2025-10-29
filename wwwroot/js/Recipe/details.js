document.addEventListener("DOMContentLoaded", () => {
    // ===== Elements =====
    const prevBtn = document.getElementById("prevStep");
    const nextBtn = document.getElementById("nextStep");
    const stepsContent = document.getElementById("stepsContent");
    const indicators = document.querySelectorAll(".indicator");
    const favoriteBtn = document.getElementById("favoriteBtn");
    const reviewBtn = document.getElementById("reviewBtn");
    const rateBtn = document.getElementById("rateBtn");

    // Modal elements
    const reviewsModal = document.getElementById("reviewsModal");
    const rateModal = document.getElementById("rateModal");
    const closeReviewsModal = document.getElementById("closeReviewsModal");
    const closeRateModal = document.getElementById("closeRateModal");

    // Reviews pagination
    const prevPageBtn = document.getElementById("prevPage");
    const nextPageBtn = document.getElementById("nextPage");
    const pageInfo = document.getElementById("pageInfo");
    const reviewsList = document.getElementById("reviewsList");

    // Rate form
    const rateForm = document.getElementById("rateForm");
    const ratingStars = document.querySelectorAll(".rating-stars .star");
    const reviewComment = document.getElementById("reviewComment");
    const cancelRateBtn = document.getElementById("cancelRate");
    const deleteRateBtn = document.getElementById("deleteRateBtn");

    // ===== State =====
    let currentStep = 1;
    let totalSteps = document.querySelectorAll(".step-item").length;
    let currentPage = 1;
    let totalPages = 1;
    let currentRating = 0;
    const recipeId = favoriteBtn ? favoriteBtn.dataset.recipeId : null;

    // ===== Steps Carousel =====
    function updateCarousel() {
        const stepItems = document.querySelectorAll(".step-item");

        // Update active step
        stepItems.forEach((item, index) => {
            item.classList.remove("active", "leaving-left", "leaving-right");
            if (index + 1 === currentStep) {
                item.classList.add("active");
            }
        });

        // Update indicators
        indicators.forEach((indicator, index) => {
            indicator.classList.toggle("active", index + 1 === currentStep);
        });

        // Update buttons
        prevBtn.disabled = currentStep === 1;
        nextBtn.disabled = currentStep === totalSteps;
    }

    function goToStep(step) {
        if (step < 1 || step > totalSteps) return;
        currentStep = step;
        updateCarousel();
    }

    function nextStep() {
        if (currentStep < totalSteps) {
            currentStep++;
            updateCarousel();
        }
    }

    function prevStep() {
        if (currentStep > 1) {
            currentStep--;
            updateCarousel();
        }
    }

    // Event listeners for carousel
    prevBtn.addEventListener("click", prevStep);
    nextBtn.addEventListener("click", nextStep);

    indicators.forEach((indicator, index) => {
        indicator.addEventListener("click", () => goToStep(index + 1));
    });

    // Keyboard navigation
    document.addEventListener("keydown", (e) => {
        if (e.key === "ArrowLeft") prevStep();
        if (e.key === "ArrowRight") nextStep();
    });

    // ===== Favorite Functionality =====
    if (favoriteBtn) {
        favoriteBtn.addEventListener("click", async () => {
            try {
                const response = await fetch(`/Recipe/ToggleFavorite?recipeId=${recipeId}`, {
                    method: "POST",
                    headers: {
                        "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value
                    }
                });

                const result = await response.json();
                if (result.success) {
                    favoriteBtn.classList.toggle("favorited", result.isFavorited);
                }
            } catch (error) {
                console.error("Error toggling favorite:", error);
            }
        });
    }

    // ===== Reviews Modal =====
    reviewBtn.addEventListener("click", () => {
        reviewsModal.style.display = "block";
        loadReviews(1);
    });

    closeReviewsModal.addEventListener("click", () => {
        reviewsModal.style.display = "none";
    });

    // ===== Reviews Pagination =====
    function loadReviews(page) {
        fetch(`/Recipe/GetReviews?recipeId=${recipeId}&page=${page}`)
            .then(response => response.json())
            .then(data => {
                renderReviews(data.reviews);
                updatePagination(data);
            })
            .catch(error => console.error("Error loading reviews:", error));
    }

    function renderReviews(reviews) {
        reviewsList.innerHTML = "";

        if (reviews.length === 0) {
            reviewsList.innerHTML = "<p style='text-align: center; color: #666; padding: 40px;'>Chưa có đánh giá nào.</p>";
            return;
        }

        reviews.forEach(review => {
            const reviewElement = createReviewElement(review);
            reviewsList.appendChild(reviewElement);
        });
    }

    function createReviewElement(review) {
        const reviewDiv = document.createElement("div");
        reviewDiv.className = "review-item";

        const stars = Array.from({length: 5}, (_, i) =>
            `<span class="star ${i < review.rating ? 'filled' : ''}">★</span>`
        ).join("");

        reviewDiv.innerHTML = `
            <div class="review-header">
                <div class="review-author">${review.userName}</div>
                <div class="review-date">${new Date(review.createdAt).toLocaleDateString('vi-VN')}</div>
            </div>
            <div class="review-rating">${stars}</div>
            <div class="review-comment">${review.comment || "Không có bình luận"}</div>
        `;

        return reviewDiv;
    }

    function updatePagination(data) {
        currentPage = data.currentPage;
        totalPages = data.totalPages;

        pageInfo.textContent = `Trang ${currentPage}`;
        prevPageBtn.disabled = !data.hasPrev;
        nextPageBtn.disabled = !data.hasNext;
    }

    prevPageBtn.addEventListener("click", () => {
        if (currentPage > 1) loadReviews(currentPage - 1);
    });

    nextPageBtn.addEventListener("click", () => {
        if (currentPage < totalPages) loadReviews(currentPage + 1);
    });

    // ===== Rate Modal =====
    if (rateBtn) {
        rateBtn.addEventListener("click", () => {
            rateModal.style.display = "block";

            // Load existing rating and comment if user has already rated
            if (window.userRating && window.userRating > 0) {
                currentRating = window.userRating;
                reviewComment.value = window.userComment || "";
                ratingStars.forEach((star, index) => {
                    star.classList.toggle("active", index < window.userRating);
                });
                // Show delete button
                deleteRateBtn.style.display = "inline-block";
            } else {
                // Reset form for new rating
                currentRating = 0;
                reviewComment.value = "";
                ratingStars.forEach(star => star.classList.remove("active"));
                // Hide delete button
                deleteRateBtn.style.display = "none";
            }
        });
    }

    closeRateModal.addEventListener("click", () => {
        closeRateModalWithoutSaving();
    });

    cancelRateBtn.addEventListener("click", () => {
        closeRateModalWithoutSaving();
    });

    function closeRateModalWithoutSaving() {
        rateModal.style.display = "none";
        // Reset to original values if user has already rated
        if (window.userRating && window.userRating > 0) {
            currentRating = window.userRating;
            reviewComment.value = window.userComment || "";
        }
    }

    // ===== Delete Review =====
    deleteRateBtn.addEventListener("click", async () => {
        if (confirm("Bạn có chắc muốn xóa đánh giá này?")) {
            try {
                const response = await fetch(`/Recipe/DeleteReview?recipeId=${recipeId}`, {
                    method: "POST",
                    headers: {
                        "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value
                    }
                });

                const result = await response.json();
                if (result.success) {
                    rateModal.style.display = "none";

                    // Reset button to not rated state
                    rateBtn.classList.remove("rated");

                    // Reset window variables
                    window.userRating = 0;
                    window.userComment = "";

                    // Reload reviews to show the updated list
                    loadReviews(1);

                    // Reload page to update average rating
                    location.reload();
                }
            } catch (error) {
                console.error("Error deleting review:", error);
            }
        }
    });

    // ===== Rating Stars =====
    ratingStars.forEach((star, index) => {
        star.addEventListener("click", () => {
            currentRating = index + 1;
            ratingStars.forEach((s, i) => {
                s.classList.toggle("active", i <= index);
            });
        });

        star.addEventListener("mouseenter", () => {
            ratingStars.forEach((s, i) => {
                s.classList.toggle("active", i <= index);
            });
        });
    });

    document.querySelector(".rating-stars").addEventListener("mouseleave", () => {
        ratingStars.forEach((s, i) => {
            s.classList.toggle("active", i < currentRating);
        });
    });

    // ===== Rate Form Submit =====
    rateForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        if (currentRating === 0) {
            alert("Vui lòng chọn số sao!");
            return;
        }

        try {
            const response = await fetch(`/Recipe/SubmitReview`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: new URLSearchParams({
                    recipeId: recipeId,
                    rating: currentRating,
                    comment: reviewComment.value
                })
            });

            const result = await response.json();
            if (result.success) {
                rateModal.style.display = "none";

                // Update the button to rated state
                rateBtn.classList.add("rated");

                // Update window variables
                window.userRating = currentRating;
                window.userComment = reviewComment.value;

                // Reload reviews to show the updated review
                loadReviews(1);

                // Reload page to update average rating
                location.reload();
            }
        } catch (error) {
            console.error("Error submitting review:", error);
        }
    });

    // ===== Modal Close on Outside Click =====
    window.addEventListener("click", (event) => {
        if (event.target === reviewsModal) {
            reviewsModal.style.display = "none";
        }
        if (event.target === rateModal) {
            rateModal.style.display = "none";
        }
    });

    // ===== Initialize =====
    updateCarousel();
});
