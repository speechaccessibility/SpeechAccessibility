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

function compareHelperEmails() {
    var email = document.getElementById('helperEmail').value;
    var compareEmail = document.getElementById('confirmHelperEmail').value;
    var emailValidation = document.getElementById('confirmHelperEmailValidation');

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
        document.getElementById("helperPhoneNumber").value = "";
    }
}

function clearHelperFields() {
    document.getElementById("helperEmail").value = "";
    document.getElementById("helperFirstName").value = "";
    document.getElementById("helperLastName").value = "";
    document.getElementById("helperPhoneNumber").value = "";
    document.getElementById("confirmHelperEmail").value = "";
}

function checkEighteenInd() {
    if (document.getElementById("eighteenOrOlderYes").checked) {
        document.getElementById("birthYearDiv").style.display = "flex";
    }
    else {
        document.getElementById("birthYearDiv").style.display = "none";
        document.getElementById("birthYear").value = "";
    }
}

function loadRegisterPage() {
    checkEighteenInd();
    checkHelperInd();
}

function loadAphasiaPage() {
    checkCorrespondence();
    checkHelperInd();
}

function loadDSCreateAccountPage() {

    checkLegalGuardian();
    checkHelperInd();
}

function loadCPRegisterPage() {
    checkLegalGuardian();
    checkHelperInd();
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

function checkDiagnois() {

    if (document.getElementById("Other").checked) {
        document.getElementById("otherDiv").style.display = "flex";
    }
    else {
        document.getElementById("otherDiv").style.display = "none";
        document.getElementById("OtherText").value = "";
    }
}

function checkCaregiverInd() {

    if (document.getElementById("caregiverYes").checked) {
        document.getElementById("caregiverDiv").style.display = "block";
        document.getElementById("contributorDiv").style.display = "none";
        document.getElementById("caregiverLegalGuardianInd").style.display = "block";
        document.getElementBy("legalGuardianInd").style.display = "none";
    }
    else if(document.getElementById("caregiverNo").checked) {
        document.getElementById("caregiverDiv").style.display = "none";
        document.getElementById("contributorDiv").style.display = "block";
        document.getElementById("legalGuardianInd").style.display = "block";
        document.getElementById("caregiverLegalGuardianInd").style.display = "none";
    }
   
}


function checkLegalGuardian() {
    if (document.getElementById("legalGuardianSelect").value == "Someone else is my legal guardian") {
        document.getElementById("legalGuardianDiv").style.display = "flex";

    }
    else {
        document.getElementById("legalGuardianDiv").style.display = "none";

    }
    
}

function checkCaregiverLegalGuardian() {

    if (document.getElementById("caregiverLegalGuardianSelect").value == "Someone else is their legal guardian") {
        document.getElementById("caregiverLegalGuardianDiv").style.display = "flex";

    }
    else {
        if (document.getElementById("assistanceAvailableNo").checked) {
        }
        document.getElementById("caregiverLegalGuardianDiv").style.display = "none";

    }

}

function checkAssistInd() {

    if (document.getElementById("assistYes").checked) {
        document.getElementById("assistanceAvailableDiv").style.display = "None";
        document.getElementById("caregiverInfo").style.display = "block";
        document.getElementById("correspondenceDiv").style.display = "block";
        document.getElementById("correspondenceLabel").innerHTML = '<span style="color:red">*</span>Would you like correspondence about this project to go to you, as the caregiver, or to the individual participating in the study?'
        document.getElementById("helperFirstNameLabel").innerHTML = "Your First Name";
        document.getElementById("helperLastNameLabel").innerHTML = "Your Last Name";
        document.getElementById("helperPhoneNumberLabel").innerHTML = "Your Phone Number";
        document.getElementById("helperEmailLabel").innerHTML = "Your Email";
        document.getElementById("helperConfirmEmailLabel").innerHTML = "Confirm Your Email Address Again";
    }
    else if (document.getElementById("assistNo").checked) {

        clearHelperFields()

        document.getElementById("assistanceAvailableDiv").style.display = "flex";
        if (!document.getElementById("assistanceAvailableYes").checked) {
            document.getElementById("caregiverInfo").style.display = "none";
            document.getElementById("correspondenceDiv").style.display = "none";
            document.getElementById("correspondenceSelf").checked = false;
            document.getElementById("contributorContactDiv").style.display = "block";
        }
        document.getElementById("helperFirstNameLabel").innerHTML = "Helper's First Name";
        document.getElementById("helperLastNameLabel").innerHTML = "Helper's Last Name";
        document.getElementById("helperPhoneNumberLabel").innerHTML = "Helper's Phone Number";
        document.getElementById("helperEmailLabel").innerHTML = "Helper's Email";
        document.getElementById("helperConfirmEmailLabel").innerHTML = "Confirm Helper's Email";
    }
}

function checkAssistanceAvailable() {

    if (document.getElementById("assistanceAvailableYes").checked) {
        document.getElementById("caregiverInfo").style.display = "block";
        document.getElementById("helperFirstNameLabel").innerHTML = "Helper's First Name";
        document.getElementById("helperLastNameLabel").innerHTML = "Helper's Last Name";
        document.getElementById("helperPhoneNumberLabel").innerHTML = "Helper's Phone Number";
        document.getElementById("helperEmailLabel").innerHTML = "Helper's Email";
        document.getElementById("helperConfirmEmailLabel").innerHTML = "Confirm Helper's Email";
        document.getElementById("correspondenceDiv").style.display = "block";
        document.getElementById("correspondenceLabel").innerHTML = '<span style="color:red">*</span>Would you like correspondence about this project to go to the helper, or to the individual participating in the study?'
    }
    else if (document.getElementById("assistanceAvailableNo").checked) {
        clearHelperFields()

        document.getElementById("correspondenceDiv").style.display = "none";
        document.getElementById("correspondenceSelf").checked = false;
        document.getElementById("contributorContactDiv").style.display = "block";
        document.getElementById("caregiverInfo").style.display = "none";
        document.getElementById("helperFirstNameLabel").innerHTML = "Your First Name";
        document.getElementById("helperLastNameLabel").innerHTML = "Your Last Name";
        document.getElementById("helperPhoneNumberLabel").innerHTML = "Your Phone Number";
        document.getElementById("helperEmailLabel").innerHTML = "Your Email";
        document.getElementById("helperConfirmEmailLabel").innerHTML = "Confirm Your Email Address Again";
    }
}

function checkCorrespondence() {

    if (document.getElementById("correspondenceSelf").checked) {
        document.getElementById("contributorContactDiv").style.display = "none";
        document.getElementById("contributorEmail").value = "";
        document.getElementById("compareEmail").value = "";
        document.getElementById("individualPhoneNumber").value = "";
    }
    else {
        document.getElementById("contributorContactDiv").style.display = "block";
    }
}

function dsRegisterLoad() {
    if (document.getElementById("downSyndromeInd").value == "Yes") {
        checkLegalGuardian();
    }
    else {
        checkAssistInd();
        checkAssistanceAvailable();
        checkCaregiverLegalGuardian();
        checkCorrespondence();
    }

    }

function speakPrompt() {
    const synth = window.speechSynthesis;
    var element = document.getElementById("speakerIcon");

    if (synth.speaking) {
        synth.cancel()
        element.classList.remove("fa-beat");
    }
    else {
        
        element.classList.add("fa-beat");
        var prompt = new SpeechSynthesisUtterance(document.getElementById("transcript").value);
        prompt.lang = 'en-US';
        prompt.text = document.getElementById("transcript").innerHTML
        synth.speak(prompt);

        prompt.addEventListener("end", (event) => {
            document.getElementById("speakerIcon").classList.remove("fa-beat");
        });
    }
   
}

function showHidePassword() {
    var password = document.getElementById("password");
    var confirmPassword = document.getElementById("confirmPassword");
    if (password.type === "password") {
        password.type = "text";
        confirmPassword.type = "text";
    } else {
        password.type = "password";
        confirmPassword.type = "password";
    }
}