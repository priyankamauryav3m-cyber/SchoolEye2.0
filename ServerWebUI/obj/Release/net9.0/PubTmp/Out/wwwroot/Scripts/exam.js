


window.fullScreen = function () {
$(document).ready(function () {
    $("#top-click").click(function () {
        $("#new-main").toggleClass("top-click");
        adjustBottomHeight();
    });
    $("#right-click").click(function () {
        $("#new-main").toggleClass("right-click");
    });
});
}

window.adjustBottomHeight = function ()
{
    function adjustHeight() {
        var windowHeight = $(window).height(); // Puri window ki height
        var topHeight = $(".top-auto-card").outerHeight(true); // Top div ki height including margin & padding
        var bottomHeight = windowHeight - topHeight - 180; // Bottom div ki required height
        $(".answer-list-box1").css("height", bottomHeight + "px");

        var windowHeight2 = $(window).height(); // Puri window ki height
        var topHeight2 = $(".top-auto-card").outerHeight(true); // Top div ki height including margin & padding
        var bottomHeight2 = windowHeight2 - topHeight2 - 238; // Bottom div ki required height
        $(".answer-list-box2").css("height", bottomHeight2 + "px");

        var windowHeight3 = $(window).height(); // Puri window ki height
        var topHeight3 = $(".top-auto-card").outerHeight(true); // Top div ki height including margin & padding
        var footerHeight3 = $(".footer-pagenation").outerHeight(true);
        var bottomHeight3 = windowHeight3 - topHeight3 - footerHeight3 - 180; // Bottom div ki required height
        $(".answer-list-box3").css("height", bottomHeight3 + "px");
    }
   
    $(document).ready(function () {
        adjustHeight(); // Page load hone par height set karega
    });

    $(window).resize(function () {
        adjustHeight(); // Jab window resize ho tab bhi height adjust karega
    });
   
}


window.scrollToDiv = (num) => {
    const targetDiv = document.getElementById("answer_" + num);
    if (targetDiv) {
        targetDiv.scrollIntoView({ behavior: "smooth", block: "start" });
    }
};


window.setupScrollListener = (dotnetHelper) => {
    let container = document.getElementById("scrollContainer");

    container.addEventListener("scroll", () => {
        let boxes = document.querySelectorAll(".answer-strat");
        let containerTop = container.scrollTop;

        boxes.forEach((box, index) => {
            let boxTop = box.offsetTop - container.offsetTop;
            let boxHeight = box.offsetHeight;

            // Check if the box is in the middle of the container
            if (containerTop >= boxTop - boxHeight / 2 && containerTop < boxTop + boxHeight / 2) {
                // Call the Blazor method to update the currentDiv
                dotnetHelper.invokeMethodAsync("UpdateCurrentDiv", index + 1);
            }
        });
    });
};

function showdropdown(selector) {
    $(selector).show();
}
function filterDropdown(inputSelector, dropdownSelector) {
    const input = $(inputSelector).val().toLowerCase();
    $(dropdownSelector + ' .dropdown-item').each(function () {
        const text = $(this).text().toLowerCase();
        $(this).toggle(text.includes(input));
    });
}
function showDropdown(selector) {
    // First hide all other open dropdowns
    $('.dropdown-list').hide(); // Hide all dropdowns
    $(selector).show(); // Show the selected dropdown
}
function filterDropdown(inputSelector, dropdownSelector) {
    const input = $(inputSelector).val().toLowerCase();
    $(dropdownSelector + ' .dropdown-item').each(function () {
        const text = $(this).text().toLowerCase();
        $(this).toggle(text.includes(input));
    });
}
function hideDropdown(selector) {
    $(selector).hide();
}

// Close dropdown when clicking outside
//$(document).on('click', function (event) {
//    if (!$(event.target).closest('.input-box').length) {
//        hideDropdown('.dropdown-list');  // Hides all dropdowns when clicking outside
//    }
//});



/*-----------------------------------Pdf Generating and Saving Method-------------------------*/

window.generateAndUploadPdf = async function (elementId, token) {
    try {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error("Element not found");
            return null;
        }
        //element.classList.add('force-page-break-style');

        // Optional: Apply to all child nodes if needed
        await waitForImagesToLoad(document.querySelector('#quillEditor'));
        /*await waitForImagesToLoad(element);*/

        const opt = {
            margin: [10, 10, 10, 10],
            filename: 'mydoc.pdf',
            image: { type: 'jpeg', quality: 0.98 },
            html2canvas: {
                scale: 1.2,
                useCORS: true,
                allowTaint: true,
                scrollY: -window.scrollY
            },
            jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' },
            pagebreak: {
                mode: ['css', 'legacy'],
                avoid: ['.keepTogether']
            }
        };

        const pdfBlob = await html2pdf().set(opt).from(element).outputPdf('blob');
        const base64Pdf = await new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onloadend = () => resolve(reader.result.split(',')[1]);
            reader.onerror = reject;
            reader.readAsDataURL(pdfBlob);
        });

        const response = await fetch("https://cqga.v3m.in/api/QuestionBank/SaveBase64Pdf", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ Base64Pdf: base64Pdf })
        });

        if (!response.ok) throw new Error(await response.text());
        return await response.text();

    } catch (err) {
        console.error("Error generating PDF:", err);
        return null;
    }
};

//async function waitForImagesToLoad(container) {
//    const images = container.querySelectorAll("img");
//    await Promise.all([...images].map(img => {
//        if (img.complete) return Promise.resolve();
//        return new Promise(resolve => {
//            img.onload = img.onerror = resolve;
//        });
//    }));
//}
function waitForImagesToLoad(container) {
    const images = container.querySelectorAll('img');
    return Promise.all(Array.from(images).map(img => {
        return new Promise(resolve => {
            if (img.complete) resolve();
            else {
                img.onload = img.onerror = resolve;
            }
        });
    }));
}
/*--------------------------Quills Editor-----------------------------------------------*/
window.quillInterop = {
    init: function () {
        const toolbarOptions = [
            ['bold', 'italic', 'underline'],
            [{ 'color': [] }, { 'background': [] }],
            [{ 'size': ['small', false, 'large', 'huge'] }],
            [{ 'align': [] }],
            ['clean']
        ];

        const container = document.querySelector("#quillEditor");
        if (!container) {
            console.error("Quill container not found.");
            return;
        }

        window.quillEditor = new Quill(container, {
            theme: "snow",
            modules: {
                toolbar: toolbarOptions
            }
        });
    },

 
    setContent: function (content) {
        console.log("Content received by Quill:", content);

        const styledContent = `
        <style>
            .ql-editor img {
                max-width: 100% !important;
                width: 100% !important;
                height: auto !important;
                display: block;
                object-fit: contain;
            }
        </style>
        ${content}
    `;

        if (window.quillEditor) {
            window.quillEditor.clipboard.dangerouslyPasteHTML(styledContent);
        } else {
            console.warn("quillEditor is not defined");
        }
    },

    getContent: function () {

        try {
            if (!window.quillEditor) {
                console.warn("⚠️ quillEditor is not defined");
                return "";
            }

            return window.quillEditor.root.innerHTML;
        } catch (e) {
            console.error("❌ getContent Error:", e);
            return "";
        }
    },
    exportPdf: async function (html) {
        try {
            const { jsPDF } = window.jspdf;

            let container = document.createElement("div");
            container.style.padding = "20px";      
            container.style.fontSize = "14px";  
            container.style.lineHeight = "1.6";
            container.style.width = "800px";     
            container.innerHTML = html;
            document.body.appendChild(container);
            const canvas = await html2canvas(container, {
                scale: 2,        
                useCORS: true
            });
            const imgData = canvas.toDataURL("image/png");
            const pdf = new jsPDF("p", "mm", "a4");
            const pdfWidth = pdf.internal.pageSize.getWidth();
            const pdfHeight = (canvas.height * pdfWidth) / canvas.width;
            pdf.addImage(imgData, "PNG", 0, 0, pdfWidth, pdfHeight);
            pdf.save("Messagesformate.pdf");
            document.body.removeChild(container);
        }
        catch (err) {
            console.error("PDF Error:", err);
            alert("PDF Generate Error: " + err.message);
        }
    }
};



window.focusElementById = (id) => {
    const el = document.getElementById(id);
    if (el) {
        el.focus();
    }
};

/*----------------------------Page Changing Alert-----------------------------------------------*/
//window.preventNavigationIfUnsaved = (shouldPrevent) => {
//    if (shouldPrevent) {
//        window.onbeforeunload = function () {
//            return "You have unsaved changes. Are you sure you want to leave?";
//        };
//    } else {
//        window.onbeforeunload = null;
//    }
//};



// ✅ ========== GET CONTENT (FINAL FIX) ==========


