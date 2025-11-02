var currentRecipeId = null;

$(document).ready(function() {
    console.log('recipelist.js loaded');
    // Init drag drop for create/edit forms
    if ($('#stepsContainer').length) {
        console.log('stepsContainer found');
        initStepDragDrop();
    }
    if ($('#ingredientsContainer').length) {
        console.log('ingredientsContainer found');
        initIngredientDragDrop();
    }

    // Real-time search
    var searchTimeout;
    $('#searchInput').on('input', function() {
        var term = $(this).val().trim();
        clearTimeout(searchTimeout);

        if (term.length > 2) {
            searchTimeout = setTimeout(function() {
                searchRecipes(term);
            }, 300);
        } else {
            $('#searchResults').hide();
        }
    });
});

    // Filters
    $('#cookingMethodFilter, #typeOfDishFilter').on('change', function() {
        filterRecipes();
    });

    // Time filter
    $('#timeFilter').on('change', function() {
        filterRecipes();
    });

    // ====== Hàm lọc công thức ======
    function filterRecipes() {
        const typeOfDish = $('#typeOfDishFilter').val();
        const cookingMethod = $('#cookingMethodFilter').val();
        const timeFilterValue = $('#timeFilter').val();
        const searchTerm = $('#searchInput').val().trim();

        // Redirect to server with filters
        const url = '/Admin/RecipeList?' +
            'searchTerm=' + encodeURIComponent(searchTerm) +
            '&typeOfDishFilter=' + encodeURIComponent(typeOfDish) +
            '&cookingMethodFilter=' + encodeURIComponent(cookingMethod) +
            '&timeFilter=' + encodeURIComponent(timeFilterValue);
        window.location.href = url;
    }

    // Edit button click handler (delegated for dynamic elements)
    $(document).off('click', '#editRecipeBtn').on('click', '#editRecipeBtn', function(e) {
    e.preventDefault();
    if (currentRecipeId) {
        // Load edit form
        $.ajax({
            url: '/Admin/GetRecipeEditForm?id=' + currentRecipeId,
            type: 'GET',
            success: function(data) {
                $('#recipeDetailsModalLabel').text('Chỉnh sửa công thức');
                $('#recipeDetailsContent').html(data);

                setTimeout(function() {
                console.log('Edit form loaded - Visible steps:', $('.step-item:visible').length, 'Visible ingredients:', $('.ingredient-item:visible').length);
                }, 200);

                // Init drag drop after content loaded
                if ($('#stepsContainer').length) {
                    initStepDragDrop();
                }
                if ($('#ingredientsContainer').length) {
                    initIngredientDragDrop();
                }
                // Change footer to save/cancel
                $('#recipeDetailsModal .modal-footer').html(`
                    <button type="button" class="btn" onclick="$('#recipeDetailsModal').modal('hide');" style="background: #A9A9A9; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px; margin-right: 10px;">Hủy</button>
                    <button type="button" class="btn" onclick="saveEditRecipe();" style="background: #28a745; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px;">Lưu</button>
                `);
            },
            error: function() {
                alert('Không thể tải form chỉnh sửa.');
            }
        });
    }
});

function searchRecipes(term) {
    $.ajax({
        url: '/Admin/SearchRecipes?term=' + encodeURIComponent(term),
        type: 'GET',
        success: function(data) {
            var results = data.results;
            var html = '';
            if (results.length > 0) {
                html += '<ul>';
                results.forEach(function(recipe) {
                    html += '<li onclick="viewRecipeDetails(' + recipe.id + ')">' + recipe.text + '</li>';
                });
                html += '</ul>';
            } else {
                html = '<p>Không tìm thấy công thức.</p>';
            }
            $('#searchResults').html(html).show();
        },
        error: function() {
            $('#searchResults').html('<p>Lỗi khi tìm kiếm.</p>').show();
        }
    });
}

function viewRecipeDetails(recipeId) {
    currentRecipeId = recipeId;
    $('#searchResults').hide();

    $.ajax({
        url: '/Admin/GetRecipeDetails?id=' + recipeId,
        type: 'GET',
        success: function(data) {
            $('#recipeDetailsModalLabel').text('Chi tiết công thức');
            $('#recipeDetailsContent').html(data);
            // Reset footer to view mode
            $('#recipeDetailsModal .modal-footer').html(`
                <a href="#" id="editRecipeBtn" class="btn btn-warning" style="background: #ffc107; color: #212529; padding: 10px 20px; border-radius: 6px; text-decoration: none; font-weight: 600; font-size: 14px;">
                    <i class="fa-solid fa-pen-to-square" style="margin-right: 5px;"></i>Sửa
                </a>
                <button type="button" class="btn btn-secondary" onclick="$('#recipeDetailsModal').modal('hide');" style="background: #A9A9A9; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px;">Đóng</button>
            `);
            $('#recipeDetailsModal').modal('show');
        },
        error: function(xhr, status, error) {
            console.error('AJAX Error:', status, error, xhr.responseText);
            alert('Không thể tải thông tin công thức. Lỗi: ' + error);
        }
    });
}

function createRecipe() {
    $.ajax({
        url: '/Admin/GetRecipeCreateForm',
        type: 'GET',
        success: function(data) {
            $('#recipeDetailsModalLabel').text('Thêm công thức');
            $('#recipeDetailsContent').html(data);

            $('#recipeDetailsModal').modal('show');

            // Init drag drop after content loaded
            if ($('#stepsContainer').length) {
                initStepDragDrop();
            }
            if ($('#ingredientsContainer').length) {
            initIngredientDragDrop();
            }
            // Change footer to save/cancel
            $('#recipeDetailsModal .modal-footer').html(`
                <button type="button" class="btn" onclick="$('#recipeDetailsModal').modal('hide');" style="background: #A9A9A9; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px; margin-right: 10px;">Hủy</button>
                <button type="button" class="btn" onclick="saveCreateRecipe();" style="background: #28a745; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px;">Thêm</button>
            `);

            setTimeout(function() {
                console.log('Create form loaded - Visible steps:', $('.step-item:visible').length, 'Visible ingredients:', $('.ingredient-item:visible').length);
            }, 200);
        },
        error: function() {
            alert('Không thể tải form thêm.');
        }
    });
}

function editRecipe(recipeId) {
    currentRecipeId = recipeId;
    $.ajax({
        url: '/Admin/GetRecipeEditForm?id=' + recipeId,
        type: 'GET',
        success: function(data) {
            $('#recipeDetailsModalLabel').text('Chỉnh sửa công thức');
            $('#recipeDetailsContent').html(data);

            $('#recipeDetailsModal').modal('show');

            // Init drag drop after content loaded
            if ($('#stepsContainer').length) {
                initStepDragDrop();
            }
            if ($('#ingredientsContainer').length) {
                initIngredientDragDrop();
            }
            // Change footer to save/cancel
            $('#recipeDetailsModal .modal-footer').html(`
                <button type="button" class="btn" onclick="$('#recipeDetailsModal').modal('hide');" style="background: #A9A9A9; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px; margin-right: 10px;">Hủy</button>
                <button type="button" class="btn" onclick="saveEditRecipe();" style="background: #28a745; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px;">Lưu</button>
            `);

            setTimeout(function() {
                console.log('Edit form loaded - Visible steps:', $('.step-item:visible').length, 'Visible ingredients:', $('.ingredient-item:visible').length);
            }, 200);
        },
        error: function() {
            alert('Không thể tải form chỉnh sửa.');
        }
    });
}

function saveCreateRecipe() {
    $('#createRecipeForm button[type="submit"]').prop('disabled', true);
    var formData = new FormData($('#createRecipeForm')[0]);
    $.ajax({
        url: '/Admin/CreateRecipeAjax',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(response) {
            $('#createRecipeForm button[type="submit"]').prop('disabled', false);
            if (response.success) {
                alert(response.message);
                $('#recipeDetailsModal').modal('hide');
                location.reload();
            } else {
                alert(response.message);
            }
        },
        error: function() {
            $('#createRecipeForm button[type="submit"]').prop('disabled', false);
            alert('Lỗi khi thêm.');
        }
    });
}

function deleteCurrentImage() {
    $('#deleteImageFlag').val('true');
    $('.current-image').hide();
}

function deleteCurrentStepImage(stepId, imageUrl) {
    // Hide the image
    var stepItem = $('button[onclick*="deleteCurrentStepImage(' + stepId + ')"]').closest('.step-item');
    stepItem.find('.current-image').hide();
    // Call ajax to delete file
    $.ajax({
        url: '/Admin/DeleteStepImage',
        type: 'POST',
        data: { stepId: stepId, imageUrl: imageUrl },
        success: function(response) {
            if (response.success) {
                console.log('Step image deleted');
            } else {
                alert('Lỗi khi xóa ảnh bước: ' + response.message);
            }
        },
        error: function() {
            alert('Lỗi khi xóa ảnh bước');
        }
    });
}

function saveEditRecipe() {
    var formData = new FormData($('#editRecipeForm')[0]);
    $.ajax({
        url: '/Admin/EditRecipeAjax',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(response) {
            if (response.success) {
                alert(response.message);
                $('#recipeDetailsModal').modal('hide');
                location.reload();
            } else {
                alert(response.message);
            }
        },
        error: function() {
            alert('Lỗi khi lưu thay đổi.');
        }
    });
}

var deleteRecipeId = null;

function confirmDeleteRecipe(recipeId, recipeTitle) {
    deleteRecipeId = recipeId;
    $('#deleteRecipeTitle').text(recipeTitle);
    $('#deleteRecipeModal').modal('show');
}

function confirmDeleteRecipeAjax() {
    if (deleteRecipeId) {
        $.ajax({
            url: '/Admin/DeleteRecipeAjax',
            type: 'POST',
            data: { id: deleteRecipeId },
            success: function(response) {
                if (response.success) {
                    alert(response.message);
                    $('#deleteRecipeModal').modal('hide');
                    location.reload();
                } else {
                    alert(response.message);
                }
            },
            error: function() {
                alert('Lỗi khi xoá.');
            }
        });
    }
}

// Handle modal backdrop click to close
$(document).on('click', '#recipeDetailsModal', function(e) {
    if (e.target === this) {
        $(this).modal('hide');
    }
});

// Step management functions
function removeStep(button) {
    var stepItem = $(button).closest('.step-item');
    stepItem.find('.is-deleted').val('true');
    stepItem.hide();
}

function removeIngredient(button) {
    var ingredientItem = $(button).closest('.ingredient-item');
    ingredientItem.find('.ingredient-is-deleted').val('true');
    ingredientItem.hide();
}

function addStep() {
    var currentStepCount = $('.step-item:visible').length;
    console.log('Current visible steps count:', currentStepCount);
    var newStepIndex = currentStepCount;
    var newStepNumber = currentStepCount + 1;

    var newStepHtml = `
        <div class='step-item' style='margin-bottom: 20px; padding: 15px; background: #1B263B; border-radius: 8px; border: 1px solid #3A475C; width: 100%; display: flex; flex-direction: row; align-items: center; gap: 10px; flex-wrap: wrap;'>
        <div style='display: flex; align-items: center; gap: 5px; min-width: 100px;'>
        <i class='fas fa-grip-vertical drag-handle' style='color: #68C69F; cursor: grab;'></i>
        <h6 style='color: #68C69F; margin: 0;'>Bước ` + newStepNumber + `</h6>
        </div>
        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
        <label style='color: #E0E1DD; font-weight: 500;'>Mô tả:</label>
        <textarea name='Steps[` + newStepIndex + `].Description' class='form-control' rows='3' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; min-height: 80px; width: 500px;'></textarea>
        <input type='hidden' name='Steps[` + newStepIndex + `].RecipeStepId' value='0' />
            <input type='hidden' name='Steps[` + newStepIndex + `].StepNumber' value='` + newStepNumber + `' />
            <input type='hidden' name='Steps[` + newStepIndex + `].IsDeleted' value='false' class='is-deleted' />
        </div>
        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
        <label style='color: #E0E1DD; font-weight: 500;'>Timer phút:</label>
        <input name='Steps[` + newStepIndex + `].TimerInMinutes' type='number' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 100px;' />
        </div>
        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
            <label style='color: #E0E1DD; font-weight: 500;'>Hình ảnh bước:</label>
            <input type='file' name='stepImages[` + newStepIndex + `]' accept='image/*' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; padding: 8px; border-radius: 4px; width: 200px;' />
        </div>
        <button type='button' class='btn btn-danger btn-sm' onclick='removeStep(this)' style='background: #D9534F; color: white; border: none; padding: 4px 8px; border-radius: 4px; font-size: 12px; margin-left: auto;'>Xóa</button>
        </div>`;

    var lastVisibleStep = $('.step-item:visible').last();
    if (lastVisibleStep.length) {
        lastVisibleStep.after(newStepHtml);
    } else {
        $('#stepsContainer').append(newStepHtml);
    }
    // Re-init drag drop for new item
    initStepDragDrop();
}

function addIngredient() {
var currentIngredientCount = $('.ingredient-item:visible').length;
console.log('Current visible ingredients count:', currentIngredientCount);
var newIngredientIndex = currentIngredientCount;
var newOrder = currentIngredientCount + 1;

// Get ingredients options from ViewBag, but since JS, hardcode or fetch.
// For simplicity, add empty select, user can select later.
var newIngredientHtml = `
<div class='ingredient-item' style='margin-bottom: 20px; padding: 15px; background: #1B263B; border-radius: 8px; border: 1px solid #3A475C; width: 100%; display: flex; flex-direction: row; align-items: center; gap: 10px; flex-wrap: wrap;'>
<div style='display: flex; align-items: center; gap: 5px; min-width: 100px;'>
<i class='fas fa-grip-vertical ingredient-drag-handle' style='color: #68C69F; cursor: grab;'></i>
<h6 style='color: #68C69F; margin: 0;'>Nguyên liệu ` + newOrder + `</h6>
</div>
<div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
<label style='color: #E0E1DD; font-weight: 500;'>Nguyên liệu:</label>
    <input type='text' name='Ingredients[` + newIngredientIndex + `].IngredientName' list='ingredientList' class='form-control' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 120px;' />
    <input type='hidden' name='Ingredients[` + newIngredientIndex + `].RecipeId' value='0' />
<input type='hidden' name='Ingredients[` + newIngredientIndex + `].IsDeleted' value='false' class='ingredient-is-deleted' />
</div>
<div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
<label style='color: #E0E1DD; font-weight: 500;'>Số lượng:</label>
<input name='Ingredients[` + newIngredientIndex + `].Quantity' type='number' step='0.01' class='form-control' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 120px;' />
</div>
<div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
<label style='color: #E0E1DD; font-weight: 500;'>Đơn vị:</label>
<select name='Ingredients[` + newIngredientIndex + `].Unit' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 100px;'>
<option value=''>-- Chọn --</option>
<option value='g'>g</option>
<option value='thìa cà phê'>thìa cà phê</option>
<option value='thìa canh'>thìa canh</option>
<option value='tép'>tép</option>
<option value='quả'>quả</option>
<option value='củ'>củ</option>
<option value='kg'>kg</option>
</select>
</div>
<div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
<label style='color: #E0E1DD; font-weight: 500;'>Ghi chú:</label>
<input name='Ingredients[` + newIngredientIndex + `].Notes' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 200px;' />
</div>
<button type='button' class='btn btn-danger btn-sm' onclick='removeIngredient(this)' style='background: #D9534F; color: white; border: none; padding: 4px 8px; border-radius: 4px; font-size: 12px; margin-left: auto;'>Xóa</button>
</div>`;

    var lastVisibleIngredient = $('.ingredient-item:visible').last();
    if (lastVisibleIngredient.length) {
        lastVisibleIngredient.after(newIngredientHtml);
    } else {
        $('#ingredientsContainer').append(newIngredientHtml);
    }
    // Populate datalist
    $('#ingredientList').html($('#ingredientOptionsTemplate').html().replace(/<option value='(\d+)'>(.*?)<\/option>/g, '<option value="$2">'));
    // Re-init drag drop
    initIngredientDragDrop();
}

// Bind add buttons after form loads
$(document).off('click', '#addStepBtn').on('click', '#addStepBtn', function() {
addStep();
});

$(document).off('click', '#addIngredientBtn').on('click', '#addIngredientBtn', function() {
addIngredient();
});

// Custom drag and drop for steps and ingredients - now called after content load

function initStepDragDrop() {

    const container = document.getElementById('stepsContainer');
    if (!container) {
        console.log('stepsContainer not found');
        return;
    }
    const items = container.querySelectorAll('.step-item');

    let draggedItem = null;

    // Allow drop on container
    container.addEventListener('dragover', function(e) {
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
    });

    items.forEach((item, index) => {
        console.log('Setting up item', index);
        item.draggable = true;

        item.addEventListener('dragstart', function(e) {
            console.log('Drag start on item', index);
            draggedItem = this;
            this.style.opacity = '0.5';
            e.dataTransfer.effectAllowed = 'move';
        });

        item.addEventListener('dragend', function(e) {
            console.log('Drag end on item', index);
            this.style.opacity = '';
            draggedItem = null;
            updateStepNumbers();
        });

        item.addEventListener('dragover', function(e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
        });

        item.addEventListener('drop', function(e) {
            console.log('Drop on item', index);
            e.preventDefault();
            if (this !== draggedItem) {
                // Swap dragged and target
                const draggedClone = draggedItem.cloneNode(true);
                const targetClone = this.cloneNode(true);

                // Replace dragged with target clone
                container.replaceChild(targetClone, draggedItem);
                // Replace target with dragged clone
                container.replaceChild(draggedClone, this);

                // Re-init events
                initStepDragDrop();
                updateStepNumbers();
            }
        });
    });
}

function initIngredientDragDrop() {
    const container = document.getElementById('ingredientsContainer');
    const items = container.querySelectorAll('.ingredient-item');
    let draggedItem = null;

    // Allow drop on container
    container.addEventListener('dragover', function(e) {
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
    });

    items.forEach(item => {
        item.draggable = true;

        item.addEventListener('dragstart', function(e) {
            draggedItem = this;
            this.style.opacity = '0.5';
            e.dataTransfer.effectAllowed = 'move';
        });

        item.addEventListener('dragend', function(e) {
            this.style.opacity = '';
            draggedItem = null;
        });

        item.addEventListener('dragover', function(e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
        });

        item.addEventListener('drop', function(e) {
            e.preventDefault();
            if (this !== draggedItem) {
                // Swap dragged and target
                const draggedClone = draggedItem.cloneNode(true);
                const targetClone = this.cloneNode(true);

                // Replace dragged with target clone
                container.replaceChild(targetClone, draggedItem);
                // Replace target with dragged clone
                container.replaceChild(draggedClone, this);

                // Re-init events
                initIngredientDragDrop();
                updateIngredientNumbers();
            }
        });
    });
}

function updateStepNumbers() {
    $('#stepsContainer .step-item').each(function(index) {
        var stepNumber = index + 1;
        $(this).find('h6').text('Bước ' + stepNumber);
        $(this).find('input[name$=".StepNumber"]').val(stepNumber);

// Update name attributes to match new order
$(this).find('textarea[name^="Steps["]').attr('name', 'Steps[' + index + '].Description');
$(this).find('input[name^="Steps["][name$=".RecipeStepId"]').attr('name', 'Steps[' + index + '].RecipeStepId');
$(this).find('input[name^="Steps["][name$=".StepNumber"]').attr('name', 'Steps[' + index + '].StepNumber');
$(this).find('input[name^="Steps["][name$=".IsDeleted"]').attr('name', 'Steps[' + index + '].IsDeleted');
$(this).find('input[name^="Steps["][name$=".TimerInMinutes"]').attr('name', 'Steps[' + index + '].TimerInMinutes');
$(this).find('input[name^="stepImages["]').attr('name', 'stepImages[' + index + ']');
});
}

function updateIngredientNumbers() {
    $('#ingredientsContainer .ingredient-item').each(function(index) {
        var order = index + 1;
        $(this).find('h6').text('Nguyên liệu ' + order);

        // Update name attributes to match new order
        $(this).find('input[name^="Ingredients["][name$=".IngredientName"]').attr('name', 'Ingredients[' + index + '].IngredientName');
        $(this).find('input[name^="Ingredients["][name$=".RecipeId"]').attr('name', 'Ingredients[' + index + '].RecipeId');
        $(this).find('input[name^="Ingredients["][name$=".IsDeleted"]').attr('name', 'Ingredients[' + index + '].IsDeleted');
        $(this).find('input[name^="Ingredients["][name$=".Quantity"]').attr('name', 'Ingredients[' + index + '].Quantity');
        $(this).find('input[name^="Ingredients["][name$=".Unit"]').attr('name', 'Ingredients[' + index + '].Unit');
        $(this).find('input[name^="Ingredients["][name$=".Notes"]').attr('name', 'Ingredients[' + index + '].Notes');
    });
}



// Ensure delete modal can be closed
$('#deleteRecipeModal').on('show.bs.modal', function() {
    $(this).removeAttr('data-backdrop data-keyboard');
});
