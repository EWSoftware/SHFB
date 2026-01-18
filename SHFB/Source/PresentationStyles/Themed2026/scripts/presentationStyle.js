//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : presentationStyle.js
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/18/2026
// Note    : Copyright 2014-2026, Eric Woodruff, All rights reserved
//           Portions Copyright 2010-2026 Microsoft, All rights reserved
//
// This file contains the methods necessary to implement the language filtering, copy to clipboard, searching, and
// table of contents options.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/04/2014  EFW  Created the code based on the MS Help Viewer script
//===============================================================================================================

// Ignore Spelling: fti json Resizer mousedown mouseup mousemove Ritchie divs

//===============================================================================================================
// This section contains the methods used to implement the language filter

// The IDs of language-specific text (LST) spans are used as dictionary keys so that we can get access to the
// spans and update them when the user selects a different language in the language filter.  The values of the
// dictionary objects are pipe separated language-specific attributes (lang1=value|lang2=value|lang3=value).
// The language ID can be specific (cs, vb, cpp, etc.) or may be a neutral entry (nu) which specifies text
// common to multiple languages.  If a language is not present and there is no neutral entry, the span is hidden
// for all languages to which it does not apply.
var allLSTSetIds = new Object();

var clipboardHandler = null, searchIndex = null, fileIndex = null, toc = null;

// Set the default language
function SetDefaultLanguage(defaultLanguage)
{
    // Create the clipboard handler
    if(typeof (Clipboard) === "function")
    {
        clipboardHandler = new ClipboardJS('.copyCode',
            {
                text: function (trigger)
                {
                    var codePanel = trigger.parentElement.parentElement;

                    if(codePanel === null || typeof(codePanel) === "undefined")
                        return "";

                    if($(codePanel).hasClass("codeHeader"))
                        codePanel = codePanel.parentElement;

                    codePanel = $(codePanel).find("code");

                    if(codePanel === null || typeof(codePanel) === "undefined")
                        return "";

                    // Toggle the icon briefly to show success
                    var iEl = $(trigger).children("span").children("i");

                    if(iEl.length !== 0)
                    {
                        $(iEl).removeClass("fa-copy").addClass("fa-check");

                        setTimeout(function ()
                        {
                            $(iEl).removeClass("fa-check").addClass("fa-copy");
                        }, 500);
                    }

                    return $(codePanel).text();
                }
            });
    }

    // Connect the language filter items to their event handler
    $(".languageFilterItem").click(function ()
    {
        SelectLanguage(this);
    });

    // Add language-specific text spans on startup.  We can't tell for sure if there are any as some
    // may be added after transformation by other components.
    $("span[data-languageSpecificText]").each(function ()
    {
        allLSTSetIds[this.id] = $(this).attr("data-languageSpecificText");
    });

    if(typeof (defaultLanguage) === "undefined" || defaultLanguage === null || defaultLanguage.trim() === "")
        defaultLanguage = "cs";

    var language = localStorage.getItem("SelectedLanguage");

    if(language === null)
        language = defaultLanguage;

    var languageFilterItem = $("[data-languageId=" + language + "]")[0]
    var currentLanguage = document.getElementById("CurrentLanguage");

    currentLanguage.innerText = languageFilterItem.innerText;

    SetSelectedLanguage(language);
}

// This is called by the language filter items to change the selected language
function SelectLanguage(languageFilterItem)
{
    var currentLanguage = document.getElementById("CurrentLanguage");

    currentLanguage.innerText = languageFilterItem.innerText;

    var language = $(languageFilterItem).attr("data-languageId");

    localStorage.setItem("SelectedLanguage", language);

    SetSelectedLanguage(language);
}

// This function executes when setting the default language and selecting a language option from the language
// filter dropdown.  The parameter is the user chosen programming language.
function SetSelectedLanguage(language)
{
    // If LST exists on the page, set the LST to show the user selected programming language
    for(var lstMember in allLSTSetIds)
    {
        var devLangSpan = document.getElementById(lstMember);

        if(devLangSpan !== null)
        {
            // There may be a carriage return before the LST span in the content so the replace function below
            // is used to trim the whitespace at the end of the previous node of the current LST node.
            if(devLangSpan.previousSibling !== null && devLangSpan.previousSibling.nodeValue !== null)
                devLangSpan.previousSibling.nodeValue = devLangSpan.previousSibling.nodeValue.replace(/[\r\n]+$/, "");

            var langs = allLSTSetIds[lstMember].split("|");
            var k = 0;
            var keyValue;

            while(k < langs.length)
            {
                keyValue = langs[k].split("=");

                if(keyValue[0] === language)
                {
                    devLangSpan.innerHTML = keyValue[1];
                    break;
                }

                k++;
            }

            // If not found, default to the neutral language.  If there is no neutral language entry, clear the
            // content to hide it.
            if(k >= langs.length)
            {
                if(language !== "nu")
                {
                    k = 0;

                    while(k < langs.length)
                    {
                        keyValue = langs[k].split("=");

                        if(keyValue[0] === "nu")
                        {
                            devLangSpan.innerHTML = keyValue[1];
                            break;
                        }

                        k++;
                    }
                }

                if(k >= langs.length)
                    devLangSpan.innerHTML = "";
            }
        }
    }

    // If code snippet groups exist, set the current language for them
    $("div[data-codeSnippetLanguage]").each(function ()
    {
        if($(this).attr("data-codeSnippetLanguage") === language)
        {
            $(this).removeClass("is-hidden");
        }
        else
        {
            $(this).addClass("is-hidden");
        }
    });
}

//===============================================================================================================
// In This Article navigation aid methods

var headerPositions = [], headerElements = [];
var quickLinks = null;

// Get the positions of the quick link header elements and set up the In This Article navigation links to
// scroll the section into view when clicked and get highlighted when the related section scrolls into view.
function InitializeQuickLinks()
{
    var sectionList = $("#InThisArticleMenu")[0];

    $(".quickLinkHeader").each(function ()
    {
        headerPositions.push(this.getBoundingClientRect().top);
        headerElements.push(this);
    });

    if(headerElements.length !== 0)
    {
        sectionList.parentElement.classList.remove("is-hidden");
        quickLinks = $(".quickLink");

        $(quickLinks[0]).addClass("is-active-quickLink");

        for(var i = 0; i < quickLinks.length; i++)
        {
            quickLinks[i].addEventListener("click", function (event)
            {
                document.removeEventListener("scroll", QuickLinkScrollHandler, true);

                for(i = 0; i < quickLinks.length; i++)
                {
                    if(quickLinks[i] === this)
                    {
                        // Adjust for sticky header height
                        const header = document.querySelector('body > header');

                        if (header)
                        {
                            const headerHeight = header.offsetHeight;
                            const elementPosition = headerElements[i].getBoundingClientRect().top;
                            const offsetPosition = window.scrollY + elementPosition - headerHeight - 10;
                            
                            window.scrollTo({
                                top: offsetPosition,
                                behavior: 'smooth'
                            });
                        }
                    }

                    quickLinks[i].classList.remove("is-active-quickLink");
                }

                this.classList.add("is-active-quickLink");

                setTimeout(function ()
                {
                    document.addEventListener("scroll", QuickLinkScrollHandler, true);
                }, 600);
            });
        }

        document.addEventListener("scroll", QuickLinkScrollHandler, true);
    }
}

// Highlight the nearest quick link as the document scrolls
function QuickLinkScrollHandler()
{
    currentScrollPosition = document.documentElement.scrollTop;
    var i = 0;

    while(i < headerPositions.length - 1)
    {
        if(currentScrollPosition <= headerPositions[i + 1])
            break;

        i++;
    }

    if(i >= headerPositions.length)
        i = headerPositions.length  - 1;

    var currentActive = document.getElementsByClassName("is-active-quickLink")[0];

    if(currentActive !== undefined)
        currentActive.classList.remove("is-active-quickLink");

    quickLinks[i].classList.add("is-active-quickLink");
}

//===============================================================================================================
// This section contains the methods necessary to implement the TOC and search functionality.

// Load the TOC information
function LoadToc()
{
    $("#ShowHideTOC").click(function () {
        $("#TOCColumn").toggleClass("is-hidden-mobile");
    });

    const tocParentId = $("meta[name='tocParentId']").attr("content");
    const selectedTopicId = $("meta[name='guid']").attr("content");

    $.ajax({
        url: "../toc.json?v={@BuildCacheIdentifier}",
        dataType: "json",
        success: function (data)
        {
            tocInfo = data;
            ParentTocElements(null, selectedTopicId, tocParentId);
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        $("#TopicBreadcrumbs").append("<strong>Unable to load TOC information: " + errorThrown + "</strong>");
        $("#TableOfContents").empty();
        $("#TableOfContents").append("This will not work if loaded from the file system directly.  Use the " +
            "View Help option to view it using a local web server instance.");
    });;
}

// Parent the TOC elements to the given element.  If null, the elements represent the root TOC for the page and
// it will also set the breadcrumb trail.
function ParentTocElements(parentElement, selectedTopicId, tocParentId)
{
    const toc = tocInfo.fragments[tocParentId];
    var root, rootTopic;

    if (parentElement === null) {
        // Add the breadcrumb links.  The first is always a root link to the first topic.
        root = tocInfo.topics[0];

        for (var i = 0; i < tocInfo.topics.length; i++) {
            if (tocInfo.topics[i].f) {
                rootTopic = tocInfo.topics[i];
                break;
            }
        }

        $("#TopicBreadcrumbs").append($("<li><a href=\"" + rootTopic.f + ".htm\">" +
            insertWordBreakOpportunities(tocInfo.titles[root.t]) + "</a></li>"));

        toc.b.forEach((i) => {
            var t = tocInfo.topics[i];

            if (t.f) {
                $("#TopicBreadcrumbs").append($("<li><a href=\"" + t.f + ".htm\">" +
                    insertWordBreakOpportunities(tocInfo.titles[t.t]) + "</a></li>"));
            }
            else {
                $("#TopicBreadcrumbs").append($("<li><p>" + insertWordBreakOpportunities(tocInfo.titles[t.t]) +
                    "</a></li>"));
            }
        });

        var topicTitle = $("meta[name='Title']").attr("content");

        $("#TopicBreadcrumbs").append($("<li><p>" + topicTitle + "</p></li>"));

        root = $("#TableOfContents");
        root.empty();
    }
    else
        root = parentElement;

    // Add the TOC child elements
    toc.t.forEach((i) => {
        var t = tocInfo.topics[i];

        if (t.c) {
            // This is a parent node with children.  They will be loaded on demand.
            root.append($("<li><a id=\"" + t.f + "\" class=\"has-submenu\" href=\"" + t.f + ".htm\">" +
                "<span data-tocParentId=\"" + t.c + "\" class=\"icon toggle\" " +
                "onclick=\"ToggleExpandCollapse(this); return false;\"><i class=\"fa fa-angle-right\"> </i></span>" +
                insertWordBreakOpportunities(tocInfo.titles[t.t]) + "</a><ul class=\"toc-menu is-hidden\"></ul></li>"));
        }
        else {
            // Just a topic, no children.
            root.append($("<li><a id=\"" + t.f + "\" href=\"" + t.f + ".htm\">" +
                insertWordBreakOpportunities(tocInfo.titles[t.t]) + "</a></li>"));
        }
    });

    if(selectedTopicId !== null)
    {
        var selectedEntry = $("#" + selectedTopicId);

        $(selectedEntry).addClass("is-active");

        if($(selectedEntry).next().children().length === 0 && $(selectedEntry).children().length !== 0 &&
          $(selectedEntry).children()[0].nodeName === "SPAN")
        {
            ToggleExpandCollapse($(selectedEntry).children()[0]);
        }
    }
}

// Toggle a TOC entry between its collapsed and expanded state loading the child elements if necessary
function ToggleExpandCollapse(item) {
    $(item).toggleClass("toggleExpanded");

    if ($(item).parent().next().children().length === 0) {
        ParentTocElements($(item).parent().next(), null, $(item).attr("data-tocParentId"))
    }

    $(item).parent().next().toggleClass("is-hidden");
}

// Insert word break opportunities into long strings to allow better wrapping in the TOC pane
function insertWordBreakOpportunities(text) {
    if (!text || text.trim() === '') {
        return text || '';
    }

    let result = '';
    let start = 0;
    let end = 0;

    while (end < text.length) {
        if (end !== 0 && end < text.length - 1) {
            const curr = text[end], next = text[end + 1], prev = text[end - 1];

            // Split between camel case words, digits, and punctuation with no intervening whitespace
            if ((/[a-z]/.test(curr) && /[A-Z]/.test(next)) ||
                (/[a-zA-Z]/.test(curr) && /\d/.test(next)) ||
                (!/[a-zA-Z0-9]/.test(curr) && !/\s/.test(prev) && /[a-zA-Z0-9]/.test(next))) {

                result += text.substring(start, end + 1) + '<wbr>';
                start = end + 1;
            }
        }

        // Skip over non-word/non-punctuation characters
        do {
            end++;
        } while (end < text.length && !/[a-zA-Z0-9]/.test(text[end]) && !/[^\w\s]/.test(text[end]));
    }

    result += text.substring(start);
    return result;
}

// Transfer to the search page from a topic
function TransferToSearchPage()
{
    var searchText = document.getElementById("SearchTerms").value.trim();

    if(searchText.length !== 0)
        document.location.replace(encodeURI("../search.html?SearchText=" + searchText));
}

// Initiate a search when the search page loads
function OnSearchPageLoad()
{
    var queryString = decodeURI(document.location.search);

    if(queryString !== "")
    {
        var idx, options = queryString.split(/[?=&]/);

        for(idx = 0; idx < options.length; idx++)
        {
            if(options[idx] === "SearchText" && idx + 1 < options.length)
            {
                document.getElementById("txtSearchText").value = options[idx + 1];
                PerformSearch();
                break;
            }
        }
    }
}

// Show or hide the search help
function ShowHideHelp()
{
    var helpInfo = document.getElementById("HelpInfo");

    if(helpInfo !== null)
    {
        if(helpInfo.classList.contains("is-hidden"))
            helpInfo.classList.remove("is-hidden");
        else
            helpInfo.classList.add("is-hidden");
    }
}

// Perform a search
function PerformSearch()
{
    let searchText = document.getElementById("txtSearchText").value;
    let searchResults = document.getElementById("searchResults");

    if(searchText.length === 0)
    {
        searchResults.innerHTML = "<strong>Nothing found</strong>";
        return;
    }

    // Get the search and file index data if not already loaded
    if(!searchIndex)
    {
        searchResults.innerHTML = "Loading index...";

        $.ajax({
            url: "searchIndex.json?v={@BuildCacheIdentifier}",
            dataType: "json",
            success: function (data)
            {
                {@LunrUseLanguagExtension}
                searchIndex = lunr.Index.load(data)
                PerformSearch();
            }
        }).fail(function(jqXHR, textStatus, errorThrown) {
            searchResults.innerHTML = "<strong>Unable to load index information: " + errorThrown +"</strong>";
        });

        return;
    }

    if(!fileIndex)
    {
        $.ajax({
            url: "fileIndex.json?v={@BuildCacheIdentifier}",
            dataType: "json",
            success: function (data)
            {
                fileIndex = data;
                PerformSearch();
            }
        }).fail(function(jqXHR, textStatus, errorThrown) {
            searchResults.innerHTML = "<strong>Unable to load index information: " + errorThrown +"</strong>";
        });

        return;
    }

    searchResults.innerHTML = "Searching...";

    try
    {
        let results = searchIndex.search(searchText);

        // Format and show the results
        let content = "<div class=\"tags\"><span class=\"tag is-info\">" + results.length + " results for \"" +
            searchText + "\"";

        if(results.length > 50)
            content += " (only showing first 50 matches)";

        content += "</span></div><dl>";

        for(let i = 0; i < 50 && i < results.length; i++)
        {
            let rIdx = parseInt(results[i].ref);
            content += "<dt><a href=\"html/" + fileIndex[rIdx].f + ".htm\" target=\"_blank\">" + fileIndex[rIdx].t +
                "</a></dt><dd>" + fileIndex[rIdx].c + "...</dd>";
        }

        content += "</dl>";

        searchResults.innerHTML = content;
    }
    catch(ex)
    {
        searchResults.innerHTML = "Query error: " + ex.message;
        return;
    }
}

//===============================================================================================================
// This section contains the methods used to handle resizing the TOC section.
// Changes made by J. Ritchie Carroll.

var resizer, tocDiv;

window.onload = function ()
{
    // Adjust the header height so that the In This Article section doesn't go under it
    const header = document.querySelector('body > header');

    if (header) {
        const headerHeight = header.offsetHeight;
        document.body.style.setProperty('--header-height', `${headerHeight}px`);
    }

    resizer = document.getElementById("Resizer");
    tocDiv = document.getElementById("TOCColumn");

    if(resizer) {
        resizer.addEventListener("mousedown", function (e)
        {
            e.preventDefault();
            document.addEventListener("mousemove", ResizerMouseMove);
            document.addEventListener("mouseup", ResizerMouseUp);
        });
    }
}

function ResizerMouseMove(e)
{
    const container = document.getElementById("ContentContainer");
    const containerRect = container.getBoundingClientRect();
    const newWidth = e.clientX - containerRect.left - 80;

    // Ensure that divs are not smaller than some arbitrary minimal width
    const minWidth = 50; // pixels
    const contentDivWidth = containerRect.width - newWidth;

    if(newWidth > minWidth && contentDivWidth > minWidth)
    {
        tocDiv.style.width = newWidth + 'px';
    }
}

function ResizerMouseUp()
{
    document.removeEventListener("mousemove", ResizerMouseMove);
    document.removeEventListener("mouseup", ResizerMouseUp);
}
