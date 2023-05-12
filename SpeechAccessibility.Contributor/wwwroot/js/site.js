/*
 MIT License

 Copyright (c) 2022 The Speech Accessibility Project,
 The Beckman Institute,
 University of Illinois, Urbana-Champaign

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
 */

$(function () {

    if (typeof document.createElement('dialog').show !== 'function') {

        var errorDiv = document.getElementById("errorDiv");
        errorDiv.innerHTML = "This browser does not support recording. Please try opening this page in a different browser or update this browser to the most recent version to continue.";

        var containerDiv = document.getElementById("container");
        containerDiv.hidden = true;
    }

        var categoryId = document.getElementById('categoryId').value;
        var subCategoryId = document.getElementById('subCategoryId').value;
        var phonationPromptCount = document.getElementById('phonationPromptCount').value;
        var showOpenEndedMessage = 'false';

        if (document.getElementById('showOpenEndedMessage') != null) {
            showOpenEndedMessage = document.getElementById('showOpenEndedMessage').value
        }
        var showSection1And2Message = 'false';
        if (document.getElementById('showSection1And2Message') != null) {
            showSection1And2Message = document.getElementById('showSection1And2Message').value;
        }

        if (categoryId == 1 && subCategoryId == 5) {
            var subCategory5Message = document.getElementById('subCategory5Message');
            if (!subCategory5Message.open) {
                subCategory5Message.showModal();
            }

        }
        else if (categoryId == 1 && subCategoryId == 4 && phonationPromptCount == 0) {
            var subCategory4Message = document.getElementById('subCategory4Message')
            if (!subCategory4Message.open) {
                subCategory4Message.showModal();
            }

        }
        if (showOpenEndedMessage == 'true') {
            var openEndedMessage = document.getElementById('openEndedMessage');
            if (!openEndedMessage.open) {
                openEndedMessage.showModal();
            }


        }
        if (showSection1And2Message == 'true') {
            var section1AndTwoMessage = document.getElementById('section1And2Message');
            if (!section1AndTwoMessage.open) {
                section1AndTwoMessage.showModal();
            }

        } 

});

function compareEmails() {
    var email = document.getElementById('contributorEmail').value;
    var compareEmail = document.getElementById('compareEmail').value;
    var emailValidation = document.getElementById('confirmEmailValidation');

    if (email.toUpperCase() != compareEmail.toUpperCase()) {
        emailValidation.innerHTML = "The email and confirmation email do not match."
    }
    else {
        emailValidation.innerHTML = ""
    }
}

function supports_dialog() {
    return document.createElement('dialog').getContext;
}

function isChecked(id) {
    var checkBox = document.getElementById(id);
    // Get the output text
    var text = document.getElementById(id + "Div");
    // If the checkbox is checked, display the output text
    if (checkBox.checked == true) {
        text.style.display = "inline-block";
        document.getElementById(id).value = true;
    } else {
        text.style.display = "none";
        document.getElementById(id).value = false;
        var yearID = id + "Year"
        var year = document.getElementById(yearID);
        year.value = "";
        if (id == "other")
        {
            var otherText = document.getElementById("otherText");
            otherText.value = "";
        }
    }
}

function checkHelperInd()
{
    if (document.getElementById("helperYes").checked) {
        document.getElementById("helperDiv").style.display = "flex";
    }
    else
    {
        document.getElementById("helperDiv").style.display = "none"; 
        document.getElementById("helperEmail").value = "";
        document.getElementById("helperFirstName").value = "";
        document.getElementById("helperLastName").value = "";
    }
}

function checkOtherRaceInd()
{
    if (document.getElementById("otherRace").checked) {
        document.getElementById("otherRaceDiv").style.display = "inline-block";
    }
    else
    {
        document.getElementById("otherRaceDiv").style.display = "none"; 
        document.getElementById("otherRaceText").value = "";
    }
}

function clearOtherRaces()
{
    if (document.getElementById("preferNotToAnswerRace").checked) {
        document.getElementById("otherRace").checked = false
        document.getElementById("otherRace").disabled = true
        document.getElementById("americanIndianOrAlaskaNative").checked = false
        document.getElementById("americanIndianOrAlaskaNative").disabled = true
        document.getElementById("asian").checked = false
        document.getElementById("asian").disabled = true
        document.getElementById("blackOrAfricanAmerican").checked = false
        document.getElementById("blackOrAfricanAmerican").disabled = true
        document.getElementById("white").checked = false
        document.getElementById("white").disabled = true

        checkOtherRaceInd();
    }
    else
    {
        document.getElementById("otherRace").disabled = false;
        document.getElementById("americanIndianOrAlaskaNative").disabled = false;
        document.getElementById("asian").disabled = false
        document.getElementById("blackOrAfricanAmerican").disabled = false
        document.getElementById("white").disabled = false
    }

}

function checkOtherLanguage() {
    if (document.getElementById("otherLanguageNo").checked) {
        document.getElementById("otherLanguageDiv").style.display = "inline-block"
    }
    else {
        document.getElementById("otherLanguageDiv").style.display = "none"
        document.getElementById("converseInOtherLanguageAge").value = "";
        document.getElementById("otherLanguage").value = ""
    }
}

function checkSpeechChange()
{
    if (document.getElementById("speechChangeYes").checked) {
        document.getElementById("speechChangeDiv").style.display = "inline-block"
    }
    else {
        document.getElementById("speechChangeDiv").style.display = "none"
        document.getElementById("speechChangeDescription").value = "";
    }
}


