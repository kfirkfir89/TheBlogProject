﻿$(document).on('change', '.btn-file :file', function () {
    var input = $(this),
        numFiles = input.get(0).files ? input.get(0).files.length : 1,
        label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
    input.trigger('fileselect', [numFiles, label]);
});

$(document).ready(function () {
    $('.btn-file :file').on('fileselect', function (event, numFiles, label) {
        console.log("teste");
        var input_label = $(this).closest('.input-group').find('.file-input-label'),
            log = numFiles > 1 ? numFiles + ' files selected' : label;

        if (input_label.length) {
            input_label.text(log);
        } else {
            if (log) alert(log);
        }
    });
});