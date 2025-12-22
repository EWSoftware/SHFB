//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : themeSwitcher.js
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/19/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the methods necessary to implement theme switching between light and dark modes
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/19/2025  EFW  Created the code
//===============================================================================================================

// Initialize the theme on page load
function initializeTheme() {
    let currentTheme = localStorage.getItem("currentTheme");

    if (!currentTheme) {
        if (window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches)
            currentTheme = "dark";

        currentTheme = "light";
    }

    applyTheme(currentTheme);

    // Set up theme switcher button
    const themeSwitcher = document.getElementById("ThemeSwitcher");

    if (themeSwitcher)
        themeSwitcher.addEventListener("click", toggleTheme);

    // Listen for system theme changes
    if (window.matchMedia) {
        const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");

        // Modern browsers
        if (mediaQuery.addEventListener) {
            mediaQuery.addEventListener("change", function (e) {
                // Only auto-switch if user hasn't manually set a preference
                if (!localStorage.getItem("currentTheme")) {
                    applyTheme(e.matches ? "dark" : "light");
                }
            });
        }
        // Older browsers
        else if (mediaQuery.addListener) {
            mediaQuery.addListener(function (e) {
                if (!localStorage.getItem("currentTheme")) {
                    applyTheme(e.matches ? "dark" : "light");
                }
            });
        }
    }
}

// Apply the theme to the document
function applyTheme(theme) {
    document.documentElement.setAttribute("data-theme", theme);
    localStorage.setItem("currentTheme", theme);
}

// Toggle between light and dark theme
function toggleTheme() {
    const currentTheme = document.documentElement.getAttribute("data-theme");
    const newTheme = currentTheme === "light" ? "dark" : "light";
    applyTheme(newTheme);
}

// Initialize the theme as early as possible to prevent flash
if (document.readyState === "loading")
    document.addEventListener("DOMContentLoaded", initializeTheme);
else
    initializeTheme();
