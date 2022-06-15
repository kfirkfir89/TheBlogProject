let index = 0;

function AddTag() {
    //get a reference to the TagEntry input element
    var tagEntry = document.getElementById("TagEntry");

    //lets use the new search function to help detect an error state
    let searchResult = search(tagEntry.value);
    if (searchResult != null) {
        //trigger my SweetAlert for whatever condition is contained in the searchResult var
        swalWithDarkButton.fire({
            html: `<span class='font-weight-bolder'>${searchResult.toUpperCase()}</span>`
        });
    }
    else {
        //create a new select option
        let newOption = new Option(tagEntry.value, tagEntry.value);
        document.getElementById("TagList").options[index++] = newOption;

    }
    //clear out the TagEntry control
    tagEntry.value = "";
    return true;

}

function AddMyTag() {
    let i = $("select[name='DatabaseTagValues'] option:selected").index()
    let selectedTag = document.getElementById("DatabaseTagValues").options[i].value;
    let newOption = new Option(selectedTag, selectedTag);
    document.getElementById("TagList").options[index++] = newOption;

    if (!tagList) return false;

    return true;
}

function DeleteTag() {

    let tagCount = 1;
    let tagList = document.getElementById("TagList");
    let databaseTagValues = document.getElementById("DatabaseTagValues");
    if (!tagList) return false;

    if (tagList.selectedIndex == -1) {
        swalWithDarkButton.fire({
            html: "<span class='font-weight-bolder'>CHOOSE A TAG BEFORE DELETING</span>"
        })
        return true;
    }

    while (tagCount > 0) {

        if (tagList.selectedIndex >= 0) {
            tagList.options[tagList.selectedIndex] = null;
            --tagCount;
        }
        else {
            tagCount = 0;
        }
        index--;
    }
}

$("form").on("submit", function () {
    $("#TagList option").prop("selected", "selected");
})

//look for the tagValues variable to see if it has data
if (tagValues != '') {
    let tagArray = tagValues.split(",");

    for (let loop = 0; loop < tagArray.length; loop++) {

        //load up or replace the opations that we have
        ReplaceTag(tagArray[loop], loop);
        index++;
    }
}

function ReplaceTag(tag, index) {

    let newOption = new Option(tag, tag)
    document.getElementById("TagList").options[index] = newOption;

}

//the search function will detect either an empty or duplicate Tag for specific post
//and return an error string if an error is detected
function search(str) {
    if (str == "") {
        return 'Empty Tags are not permitted';
    }

    var tagEl = document.getElementById("TagList");
    if (tagEl) {
        let options = tagEl.options;
        for (let index = 0; index < options.length; index++) {
            if (options[index].value == str)
                return `The Tag #${str} was detected as a Duplicate and not Permitted`;
        }
    }
}

const swalWithDarkButton = Swal.mixin({
    customClass: {
        confirmButton: 'btn btn-danger btn-sm btn-block dtn-outline-dark'
    },
    imageUrl: '/assets/img/opss.png',
    imageWidth: 200,
    imageHeight: 200,
    timer: 5000,
    buttonsStyling: false
});


