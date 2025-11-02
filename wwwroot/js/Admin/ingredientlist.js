var currentIngredientId = null; // Store current ingredient ID

function removeVietnameseAccents(str) {
    return str.normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D');
}

$(document).ready(function() {
    // Clear all filters button
    $('#clearAllBtn').on('click', function() {
        window.location.href = '/Admin/IngredientList?searchTerm=&typeFilter=';
    });

    // Auto-generate ViNameWithoutAccents when typing Name
    $(document).on('input', '#ingredientName', function() {
        var name = $(this).val();
        var noAccent = removeVietnameseAccents(name);
        $('#viNameWithoutAccents').val(noAccent);
    });

    // Edit button click handler (delegated for dynamic elements)
    $(document).on('click', '#editIngredientBtn', function(e) {
        e.preventDefault();
        if (currentIngredientId) {
            // Load edit form
            $.ajax({
                url: '/Admin/GetIngredientEditForm?id=' + currentIngredientId,
                type: 'GET',
                success: function(data) {
                    $('#ingredientDetailsModalLabel').text('Chỉnh sửa nguyên liệu');
                    $('#ingredientDetailsContent').html(data);
                    // Change footer to save/cancel
                    $('#ingredientDetailsModal .modal-footer').html(`
                        <button type="button" class="btn" onclick="$('#ingredientDetailsModal').modal('hide');" style="background: #A9A9A9; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px; margin-right: 10px;">Hủy</button>
                        <button type="button" class="btn" onclick="saveEditIngredient();" style="background: #28a745; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px;">Lưu</button>
                    `);
                },
                error: function() {
                    alert('Không thể tải form chỉnh sửa.');
                }
            });
        }
    });

    // Type filter
    $('#typeFilter').on('change', function() {
        var type = $(this).val();
    var searchTerm = $('#searchInput').val().trim();
    // Redirect to action with new filter
        window.location.href = '/Admin/IngredientList?searchTerm=' + encodeURIComponent(searchTerm) + '&typeFilter=' + encodeURIComponent(type);
    });

    // Real-time search
    var searchTimeout;
    $('#searchInput').on('input', function() {
    var term = $(this).val().trim().toLowerCase();
    clearTimeout(searchTimeout);

    searchTimeout = setTimeout(function() {
    // Filter table rows
            $('.ingredient-table tbody tr').each(function() {
    var row = $(this);
    var name = row.find('td:nth-child(2)').text().toLowerCase(); // Tên tiếng Việt
    var enName = row.find('td:nth-child(3)').text().toLowerCase(); // Tên tiếng Anh
    var description = row.find('td:nth-child(4)').text().toLowerCase(); // Mô tả

    // Normalize search term (remove accents for Vietnamese)
    var normalizedTerm = term.normalize('NFD').replace(/[\u0300-\u036f]/g, '');

    // Check if any field contains the search term (with or without accents)
    var matches = term.length === 0 ||
        name.includes(term) ||
    enName.includes(term) ||
        description.includes(term) ||
            name.normalize('NFD').replace(/[\u0300-\u036f]/g, '').includes(normalizedTerm) ||
                    description.normalize('NFD').replace(/[\u0300-\u036f]/g, '').includes(normalizedTerm);

        if (matches) {
        row.show();
        } else {
        row.hide();
        }
        });

            // Update pagination info (hide pagination when searching)
            if (term.length > 0) {
                $('.pagination').hide();
            } else {
                $('.pagination').show();
            }
        }, 300);
    });
});

function viewIngredientDetails(ingredientId) {
currentIngredientId = ingredientId; // Store for edit button

$.ajax({
url: '/Admin/GetIngredientDetails?id=' + ingredientId,
type: 'GET',
data: { id: ingredientId },
success: function(data) {
$('#ingredientDetailsModalLabel').text('Chi tiết nguyên liệu');
$('#ingredientDetailsContent').html(data);
// Reset footer to view mode
$('#ingredientDetailsModal .modal-footer').html(`
    <a href="#" id="editIngredientBtn" class="btn btn-warning" style="background: #ffc107; color: #212529; padding: 10px 20px; border-radius: 6px; text-decoration: none; font-weight: 600; font-size: 14px; border: none;">
        <i class="fa-solid fa-pen-to-square" style="margin-right: 5px;"></i>Sửa
    </a>
    <button type="button" class="btn btn-secondary" onclick="$('#ingredientDetailsModal').modal('hide');" style="background: #A9A9A9; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px;">Đóng</button>
`);
$('#ingredientDetailsModal').modal('show');
},
error: function(xhr, status, error) {
console.error('AJAX Error:', status, error);
    alert('Không thể tải thông tin nguyên liệu. Lỗi: ' + error);
    }
    });
}

function createIngredient() {
    $.ajax({
        url: '/Admin/GetIngredientCreateForm',
        type: 'GET',
        success: function(data) {
            $('#ingredientDetailsModalLabel').text('Thêm nguyên liệu');
            $('#ingredientDetailsContent').html(data);
            // Change footer to save/cancel
            $('#ingredientDetailsModal .modal-footer').html(`
                <button type="button" class="btn" onclick="$('#ingredientDetailsModal').modal('hide');" style="background: #A9A9A9; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px; margin-right: 10px;">Hủy</button>
                <button type="button" class="btn" onclick="saveCreateIngredient();" style="background: #28a745; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px;">Thêm</button>
            `);
            $('#ingredientDetailsModal').modal('show');
        },
        error: function() {
            alert('Không thể tải form thêm.');
        }
    });
}

function editIngredient(ingredientId) {
    currentIngredientId = ingredientId;
    $.ajax({
        url: '/Admin/GetIngredientEditForm?id=' + ingredientId,
        type: 'GET',
        success: function(data) {
            $('#ingredientDetailsModalLabel').text('Chỉnh sửa nguyên liệu');
            $('#ingredientDetailsContent').html(data);
            // Change footer to save/cancel
            $('#ingredientDetailsModal .modal-footer').html(`
                <button type="button" class="btn" onclick="$('#ingredientDetailsModal').modal('hide');" style="background: #A9A9A9; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px; margin-right: 10px;">Hủy</button>
                <button type="button" class="btn" onclick="saveEditIngredient();" style="background: #28a745; color: white; padding: 10px 20px; border-radius: 6px; border: none; font-weight: 600; font-size: 14px;">Lưu</button>
            `);
            $('#ingredientDetailsModal').modal('show');
        },
        error: function() {
            alert('Không thể tải form chỉnh sửa.');
        }
    });
}

// Handle modal backdrop click to close
$(document).on('click', '#ingredientDetailsModal', function(e) {
    if (e.target === this) {
        $(this).modal('hide');
    }
});

var deleteIngredientId = null;

function confirmDeleteIngredient(ingredientId, ingredientName) {
    deleteIngredientId = ingredientId;
    $('#deleteIngredientName').text(ingredientName);
    $('#deleteIngredientModal').modal('show');
}

function confirmDelete() {
    if (deleteIngredientId) {
        console.log('Deleting ID:', deleteIngredientId);
        $.ajax({
            url: '/Admin/DeleteIngredientAjax',
            type: 'POST',
            data: { id: deleteIngredientId },
            success: function(response) {
                if (response.success) {
                    alert(response.message);
                    $('#deleteIngredientModal').modal('hide');
                    location.reload();
                } else {
                    alert(response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('AJAX Error:', status, error, xhr.responseText);
                alert('Lỗi khi xoá: ' + error + ' - ' + xhr.responseText);
            }
        });
    }
}

// Ensure delete modal can be closed
$('#deleteIngredientModal').on('show.bs.modal', function() {
    $(this).removeAttr('data-backdrop data-keyboard');
});

function saveCreateIngredient() {
    var formData = new FormData($('#createIngredientForm')[0]);
    $.ajax({
        url: '/Admin/CreateIngredientAjax',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(response) {
            if (response.success) {
                alert(response.message);
                $('#ingredientDetailsModal').modal('hide');
                location.reload(); // Reload page to show new data
            } else {
                alert(response.message);
            }
        },
        error: function() {
            alert('Lỗi khi thêm.');
        }
    });
}

function saveEditIngredient() {
    var formData = new FormData($('#editIngredientForm')[0]);
    $.ajax({
        url: '/Admin/EditIngredientAjax',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(response) {
            if (response.success) {
                alert(response.message);
                $('#ingredientDetailsModal').modal('hide');
                location.reload(); // Reload page to show updated data
            } else {
                alert(response.message);
            }
        },
        error: function() {
            alert('Lỗi khi lưu thay đổi.');
        }
    });
}


