(() => {
    'use strict'

    // CONSTANTS
    const HTML_ELEMENT = document.documentElement
    const LANG = "appLang"
    const LANG_ATTRIBUTE = "lang"
    const LANGUAGES = {
        // default language
        en: "English",
        // add supported languages below
        pl: "Polski"
    };
    const LANG_DEFAULT = Object.keys(LANGUAGES)[0]


    // FUNCTIONS
    // Function that gets the LANGUAGE keys
    window.getAppLanguageKeys = () => Object.keys(LANGUAGES)

    // Function that gets the LANGUAGE values
    window.getAppLanguageValues = () => Object.values(LANGUAGES)

    // Function that gets the stored language from the localStorage
    window.getStoredLanguage = () => localStorage.getItem(LANG)

    // Function that checks the preferred language using navigator.language
    window.getBrowsersLanguage = () => (navigator.language || navigator.userLanguage).split('-')[0]

    // Function that gets the language that is currently set 
    window.getCurrentLanguage = () => HTML_ELEMENT.getAttribute(LANG_ATTRIBUTE).split('-')[0]

    // Function that gets the preferred App language
    window.getPreferredAppLanguage = () => {
        const storedLang = getStoredLanguage() ?? getBrowsersLanguage() ?? LANG_DEFAULT
        return storedLang && LANGUAGES.hasOwnProperty(storedLang) ? storedLang : LANG_DEFAULT
    }

    // Function that sets the App language
    window.setAppLanguage = lang => {
        HTML_ELEMENT.setAttribute(LANG_ATTRIBUTE, lang)
        localStorage.setItem(LANG, lang)
    }


    // MAIN (sets language on initialization)
    setAppLanguage(getPreferredAppLanguage())
})()