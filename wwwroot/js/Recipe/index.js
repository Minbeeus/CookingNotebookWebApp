document.addEventListener("DOMContentLoaded", () => {
const typeOfDishList = document.getElementById("typeOfDishList");
const cookingMethodList = document.getElementById("cookingMethodList");
const timeList = document.getElementById("timeList");
const gridViewBtn = document.getElementById("gridView");
const listViewBtn = document.getElementById("listView");
const recipeList = document.querySelector(".recipe-list");
const modal = document.getElementById("recipeModal");
const closeModal = document.querySelector(".close-modal");
const searchInput = document.getElementById("searchInput");
const searchBtn = document.getElementById("searchBtn");
const clearFiltersBtn = document.getElementById("clearFilters");



let cards = document.querySelectorAll(".recipe-card");

// ====== Hàm cập nhật sự kiện click mở modal ======
function refreshCards() {
cards = document.querySelectorAll(".recipe-link");
cards.forEach(card => {
card.addEventListener("click", async () => {
const recipeId = card.getAttribute("data-recipe-id");

// Gọi API để lấy chi tiết công thức
try {
const response = await fetch(`/Recipe/GetRecipeDetails?id=${recipeId}`);
if (response.ok) {
const data = await response.json();
populateRecipeModal(data);
modal.classList.add("active");
} else {
alert("Không thể tải thông tin công thức. Vui lòng thử lại.");
}
} catch (error) {
console.error('Error loading recipe details:', error);
alert("Có lỗi xảy ra khi tải thông tin công thức.");
}
});
});
}

// ====== Hàm điền dữ liệu vào modal ======
function populateRecipeModal(data) {
if (!data) return;

// Header info
const modalImage = document.getElementById("modalImage");
const modalTitle = document.getElementById("modalTitle");
const modalPrepTime = document.getElementById("modalPrepTime");
const modalCookTime = document.getElementById("modalCookTime");
const modalServings = document.getElementById("modalServings");
const modalRating = document.getElementById("modalRating");
const modalDescription = document.getElementById("modalDescription");

if (modalImage) modalImage.src = data.imageUrl || "";
if (modalTitle) modalTitle.textContent = data.title || "Không có tiêu đề";
if (modalPrepTime) modalPrepTime.textContent = (data.prepTime || 0) + " phút chuẩn bị";
if (modalCookTime) modalCookTime.textContent = (data.cookTime || 0) + " phút nấu";
if (modalServings) modalServings.textContent = (data.servings || 1) + " người ăn";
if (modalRating) modalRating.textContent = ((data.averageRating || 0).toFixed(1)) + " ⭐ (" + (data.reviewCount || 0) + " đánh giá)";

// Description
if (modalDescription) modalDescription.textContent = data.description || "Không có mô tả.";

// Ingredients
const ingredientsDiv = document.getElementById("modalIngredients");
const ingredientsList = document.getElementById("modalIngredientsList");
if (data.ingredients && data.ingredients.length > 0 && ingredientsList) {
ingredientsList.innerHTML = "";
data.ingredients.forEach(ingredient => {
const li = document.createElement("li");
li.className = "ingredient-item";
li.innerHTML = `
<span class="ingredient-name">${ingredient.name}</span>
<span class="ingredient-quantity">${ingredient.quantity} ${ingredient.unit}</span>
${ingredient.notes ? `<span class="ingredient-notes">(${ingredient.notes})</span>` : ""}
`;
ingredientsList.appendChild(li);
});
const ingredientsTitle = document.getElementById("modalIngredientsTitle");
if (ingredientsTitle) ingredientsTitle.textContent = `Nguyên liệu (${data.servings} người ăn)`;
if (ingredientsDiv) ingredientsDiv.style.display = "block";
} else {
if (ingredientsDiv) ingredientsDiv.style.display = "none";
}

// Steps
const stepsDiv = document.getElementById("modalSteps");
const stepsCarousel = document.getElementById("modalStepsCarousel");
const stepsContent = document.getElementById("modalStepsContent");
const stepsIndicators = document.getElementById("modalStepsIndicators");
const noSteps = document.getElementById("modalNoSteps");

if (data.steps && data.steps.length > 0 && stepsContent && stepsIndicators) {
stepsContent.innerHTML = "";
stepsIndicators.innerHTML = "";

data.steps.forEach((step, index) => {
const stepDiv = document.createElement("div");
stepDiv.className = `step-item ${index === 0 ? "active" : ""}`;
stepDiv.setAttribute("data-step", step.stepNumber);
stepDiv.innerHTML = `
<div class="step-image">
${step.imageUrl ? `<img src="${step.imageUrl}" alt="Bước ${step.stepNumber}" />` : `<img src="${data.imageUrl}" alt="Bước ${step.stepNumber}" />`}
</div>
<div class="step-content">
<div class="step-number">Bước ${step.stepNumber}</div>
<div class="step-description">${step.description || `Bước ${step.stepNumber}: Mô tả chưa được cung cấp.`}</div>
${step.timerInMinutes ? `<div class="step-timer">⏱️ ${step.timerInMinutes} phút</div>` : ""}
</div>
`;
stepsContent.appendChild(stepDiv);

// Indicators
const indicator = document.createElement("span");
indicator.className = `indicator ${index === 0 ? "active" : ""}`;
indicator.setAttribute("data-step", step.stepNumber);
stepsIndicators.appendChild(indicator);
});

if (stepsCarousel) stepsCarousel.style.display = "flex";
if (stepsIndicators) stepsIndicators.style.display = "flex";
if (noSteps) noSteps.style.display = "none";
if (stepsDiv) stepsDiv.style.display = "block";

// Setup step navigation
setupStepNavigation();
} else {
if (stepsCarousel) stepsCarousel.style.display = "none";
if (stepsIndicators) stepsIndicators.style.display = "none";
if (noSteps) noSteps.style.display = "block";
if (stepsDiv) stepsDiv.style.display = "block";
}

// Notes
const notesDiv = document.getElementById("modalNotes");
const notesContent = document.getElementById("modalNotesContent");
if (data.note && notesContent) {
notesContent.textContent = data.note;
if (notesDiv) notesDiv.style.display = "block";
} else {
if (notesDiv) notesDiv.style.display = "none";
}

// Action buttons
const favoriteBtn = document.getElementById("modalFavoriteBtn");
if (favoriteBtn) {
    favoriteBtn.setAttribute("data-recipe-id", data.recipeId);
    favoriteBtn.classList.toggle("favorited", data.isFavorited);
}

const reviewBtn = document.getElementById("modalReviewBtn");
if (reviewBtn) {
    reviewBtn.setAttribute("data-recipe-id", data.recipeId);
}

const rateBtn = document.getElementById("modalRateBtn");
if (rateBtn) {
    rateBtn.setAttribute("data-recipe-id", data.recipeId);
    rateBtn.classList.toggle("rated", data.userRating > 0);
}

// Store rating data for JavaScript
window.userRating = data.userRating || 0;
window.userComment = data.userComment || "";
}

// ====== Hàm setup navigation cho steps ======
function setupStepNavigation() {
const prevBtn = document.getElementById("modalPrevStep");
const nextBtn = document.getElementById("modalNextStep");
const indicators = document.querySelectorAll("#modalStepsIndicators .indicator");
let currentStep = 1;

function updateStepVisibility() {
const steps = document.querySelectorAll("#modalStepsContent .step-item");
steps.forEach(step => {
step.classList.toggle("active", parseInt(step.dataset.step) === currentStep);
});
indicators.forEach((indicator, index) => {
indicator.classList.toggle("active", index + 1 === currentStep);
});
prevBtn.disabled = currentStep === 1;
nextBtn.disabled = currentStep === steps.length;
}

prevBtn.addEventListener("click", () => {
if (currentStep > 1) {
currentStep--;
updateStepVisibility();
}
});

nextBtn.addEventListener("click", () => {
const steps = document.querySelectorAll("#modalStepsContent .step-item");
if (currentStep < steps.length) {
currentStep++;
updateStepVisibility();
}
});

indicators.forEach((indicator, index) => {
indicator.addEventListener("click", () => {
currentStep = index + 1;
updateStepVisibility();
});
});

updateStepVisibility();
}

refreshCards();

// ====== Bộ lọc ======
function addFilterListeners(listElement, filterName) {
    const lis = listElement.querySelectorAll("li");
    lis.forEach(li => {
        li.addEventListener("click", () => {
            // Remove active from siblings
            lis.forEach(sib => sib.classList.remove("active"));
            // Add active to clicked
            li.classList.add("active");
            // Filter
            filterRecipes();
        });
    });
}

addFilterListeners(typeOfDishList, "typeOfDishFilter");
addFilterListeners(cookingMethodList, "cookingMethodFilter");
addFilterListeners(timeList, "timeFilter");

    // ====== Hàm lọc công thức ======
    function filterRecipes() {
    const typeOfDish = typeOfDishList.querySelector(".active").dataset.value;
    const cookingMethod = cookingMethodList.querySelector(".active").dataset.value;
    const timeFilterValue = timeList.querySelector(".active").dataset.value;

    // Redirect to server with filters
    const url = '/Recipe/Index?' +
    'typeOfDishFilter=' + encodeURIComponent(typeOfDish) +
    '&cookingMethodFilter=' + encodeURIComponent(cookingMethod) +
    '&timeFilter=' + encodeURIComponent(timeFilterValue);
    window.location.href = url;
    }

    // ====== Chuyển đổi dạng hiển thị ======
    gridViewBtn.addEventListener("click", () => {
        gridViewBtn.classList.add("active");
        listViewBtn.classList.remove("active");
        recipeList.classList.add("grid-view");
        recipeList.classList.remove("list-view");
    });

    listViewBtn.addEventListener("click", () => {
        listViewBtn.classList.add("active");
        gridViewBtn.classList.remove("active");
        recipeList.classList.add("list-view");
        recipeList.classList.remove("grid-view");
    });

    // ====== Tìm kiếm ======
    function performSearch() {
    const searchTerm = searchInput.value.toLowerCase().trim();

    cards.forEach(card => {
    const recipeCard = card.querySelector(".recipe-card");
    const title = (recipeCard.dataset.title || "").toLowerCase();
    const description = (recipeCard.dataset.description || "").toLowerCase();
            const author = (recipeCard.dataset.author || "").toLowerCase();

    const match = title.includes(searchTerm) ||
    description.includes(searchTerm) ||
                          author.includes(searchTerm);

        card.style.display = match ? "block" : "none";
        });
    }

    searchInput.addEventListener("input", performSearch);
    searchBtn.addEventListener("click", performSearch);

    // ====== Xoá lọc ======
    clearFiltersBtn.addEventListener("click", () => {
    // Redirect to Recipe/Index without any filters
    window.location.href = '/Recipe/Index';
    });

    // ====== Modal action buttons ======
    const modalFavoriteBtn = document.getElementById("modalFavoriteBtn");
    const modalReviewBtn = document.getElementById("modalReviewBtn");
    const modalRateBtn = document.getElementById("modalRateBtn");

    modalFavoriteBtn?.addEventListener("click", async function() {
        const recipeId = this.getAttribute("data-recipe-id");
        try {
            const response = await fetch('/Recipe/ToggleFavorite', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `recipeId=${recipeId}`
            });
            const result = await response.json();
            if (result.success) {
                this.classList.toggle("favorited", result.isFavorited);
            }
        } catch (error) {
            console.error('Error toggling favorite:', error);
        }
    });

    modalReviewBtn?.addEventListener("click", function() {
        const recipeId = this.getAttribute("data-recipe-id");
        // Show reviews modal
        const reviewsModal = document.getElementById("reviewsModal");
        reviewsModal.style.display = "block";
        loadReviews(recipeId, 1);
    });

    modalRateBtn?.addEventListener("click", function() {
        const recipeId = this.getAttribute("data-recipe-id");
        // Show rate modal
        const rateModal = document.getElementById("rateModal");
        rateModal.style.display = "block";
        // Set current rating
        const stars = rateModal.querySelectorAll(".star");
        stars.forEach((star, index) => {
            star.classList.toggle("active", index < window.userRating);
        });
        rateModal.querySelector("textarea").value = window.userComment;
    });

    // ====== Close modals ======
    document.querySelectorAll(".close-modal").forEach(closeBtn => {
    closeBtn.addEventListener("click", function() {
    const modal = this.closest(".modal");
        if (modal.id === "recipeModal") {
                modal.classList.remove("active");
            } else {
                modal.style.display = "none";
            }
        });
    });

    // Handle click on modal background to close modal
    modal.addEventListener("click", (event) => {
        if (event.target === modal) {
            modal.classList.remove("active");
        }
    });

    // ====== Hàm tải reviews ======
    function loadReviews(recipeId, page) {
        fetch(`/Recipe/GetReviews?recipeId=${recipeId}&page=${page}`)
            .then(response => response.json())
            .then(data => {
                const reviewsList = document.getElementById("reviewsList");
                reviewsList.innerHTML = "";

                data.reviews.forEach(review => {
                    const reviewDiv = document.createElement("div");
                    reviewDiv.className = "review-item";
                    reviewDiv.innerHTML = `
                        <div class="review-header">
                            <span class="review-author">${review.userName}</span>
                            <span class="review-date">${new Date(review.createdAt).toLocaleDateString('vi-VN')}</span>
                        </div>
                        <div class="review-rating">${'★'.repeat(review.rating)}${'☆'.repeat(5-review.rating)}</div>
                        <p class="review-comment">${review.comment || 'Không có bình luận'}</p>
                    `;
                    reviewsList.appendChild(reviewDiv);
                });

                // Pagination
                document.getElementById("prevPage").disabled = !data.hasPrev;
                document.getElementById("nextPage").disabled = !data.hasNext;
                document.getElementById("pageInfo").textContent = `Trang ${data.currentPage}`;

                document.getElementById("prevPage").onclick = () => loadReviews(recipeId, data.currentPage - 1);
                document.getElementById("nextPage").onclick = () => loadReviews(recipeId, data.currentPage + 1);
            })
            .catch(error => console.error('Error loading reviews:', error));
    }

    // ====== Rate form handling ======
    const rateForm = document.getElementById("rateForm");
    rateForm?.addEventListener("submit", async function(e) {
        e.preventDefault();
        const recipeId = document.getElementById("modalRateBtn").getAttribute("data-recipe-id");
        const rating = document.querySelectorAll("#rateForm .star.active").length;
        const comment = document.getElementById("reviewComment").value;

        try {
            const response = await fetch('/Recipe/SubmitReview', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `recipeId=${recipeId}&rating=${rating}&comment=${encodeURIComponent(comment)}`
            });
            const result = await response.json();
            if (result.success) {
                document.getElementById("rateModal").style.display = "none";
                // Refresh recipe modal to show updated rating
                const recipeResponse = await fetch(`/Recipe/GetRecipeDetails?id=${recipeId}`);
                if (recipeResponse.ok) {
                    const data = await recipeResponse.json();
                    populateRecipeModal(data);
                }
            }
        } catch (error) {
            console.error('Error submitting review:', error);
        }
    });

    // Star rating interaction
    document.querySelectorAll("#rateForm .star").forEach((star, index) => {
        star.addEventListener("click", function() {
            document.querySelectorAll("#rateForm .star").forEach((s, i) => {
                s.classList.toggle("active", i <= index);
            });
        });
    });

    // Delete review
    document.getElementById("deleteRateBtn")?.addEventListener("click", async function() {
        const recipeId = document.getElementById("modalRateBtn").getAttribute("data-recipe-id");
        try {
            const response = await fetch('/Recipe/DeleteReview', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `recipeId=${recipeId}`
            });
            const result = await response.json();
            if (result.success) {
                document.getElementById("rateModal").style.display = "none";
                // Refresh recipe modal
                const recipeResponse = await fetch(`/Recipe/GetRecipeDetails?id=${recipeId}`);
                if (recipeResponse.ok) {
                    const data = await recipeResponse.json();
                    populateRecipeModal(data);
                }
            }
        } catch (error) {
            console.error('Error deleting review:', error);
        }
    });
});
