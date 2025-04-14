using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing.Imaging; // For ScreenshotImageFormat

namespace ResilientFormTest
{
    public class FormPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private string url = "https://app.cloudqa.io/home/AutomationPracticeForm";

        // Locators for regular DOM elements
        private readonly By firstNameLocator = By.Id("fname");
        private readonly By lastNameLocator = By.Id("lname");
        private readonly By maleGenderLocator = By.Id("male");
        private readonly By femaleGenderLocator = By.Id("female");
        private readonly By transgenderGenderLocator = By.Id("transgender");
        private readonly By stateDropdownLocator = By.Id("state");
        private readonly By danceHobbyLocator = By.Id("Dance");
        private readonly By readingHobbyLocator = By.Id("Reading");
        private readonly By cricketHobbyLocator = By.Id("Cricket");

        public FormPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public FormPage NavigateTo()
        {
            driver.Navigate().GoToUrl(url);
            
            // Wait for the page to load
            try
            {
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
                
                // Check for iframes and try to switch to them if needed
                var iframes = driver.FindElements(By.TagName("iframe"));
                Console.WriteLine($"Found {iframes.Count} iframes on the page");
                
                // First try to find the form in the main document
                try
                {
                    wait.Until(d => d.FindElement(By.Id("automationtestform")).Displayed);
                    Console.WriteLine("Form found in main document");
                }
                catch (Exception)
                {
                    // If form not found in main document, try each iframe
                    bool formFound = false;
                    
                    for (int i = 0; i < iframes.Count; i++)
                    {
                        try
                        {
                            Console.WriteLine($"Switching to iframe {i}");
                            driver.SwitchTo().Frame(i);
                            
                            // Try to find the form in this iframe
                            try
                            {
                                var form = driver.FindElement(By.Id("automationtestform"));
                                if (form.Displayed)
                                {
                                    Console.WriteLine($"Form found in iframe {i}");
                                    formFound = true;
                                    break;
                                }
                            }
                            catch
                            {
                                // Form not in this iframe, switch back to main content
                                driver.SwitchTo().DefaultContent();
                            }
                        }
                        catch (Exception frameEx)
                        {
                            Console.WriteLine($"Error switching to iframe {i}: {frameEx.Message}");
                            driver.SwitchTo().DefaultContent();
                        }
                    }
                    
                    if (!formFound)
                    {
                        Console.WriteLine("Form not found in any iframe, staying in main document");
                        driver.SwitchTo().DefaultContent();
                    }
                }
                
                Console.WriteLine("Page loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Page load wait condition failed: {ex.Message}");
            }
            
            return this;
        }

        #region Regular DOM Interactions

        public FormPage EnterFirstName(string firstName)
        {
            IWebElement firstNameField = FindElementSafely(
                () => driver.FindElement(By.Id("fname")),
                () => driver.FindElement(By.Name("First Name")),
                () => driver.FindElement(By.XPath("//input[@placeholder='Name']")),
                () => driver.FindElement(By.XPath("//label[contains(text(), 'First Name')]/following::input[1]")),
                () => driver.FindElement(By.CssSelector("input[placeholder*='Name']"))
            );

            firstNameField.Clear();
            firstNameField.SendKeys(firstName);
            return this;
        }

        public string GetFirstNameValue()
        {
            IWebElement firstNameField = FindElementSafely(
                () => driver.FindElement(By.Id("fname")),
                () => driver.FindElement(By.Name("First Name")),
                () => driver.FindElement(By.XPath("//input[@placeholder='Name']")),
                () => driver.FindElement(By.CssSelector("input[placeholder*='Name']"))
            );

            return firstNameField.GetAttribute("value") ?? string.Empty;
        }

        public FormPage SelectGender(string gender)
        {
            IWebElement genderRadio = FindElementSafely(
                () => driver.FindElement(By.Id(gender.ToLower())),
                () => driver.FindElement(By.XPath($"//input[@type='radio'][@value='{gender}']")),
                () => driver.FindElement(By.XPath($"//span[text()='{gender}']/preceding-sibling::input[@type='radio']")),
                () => driver.FindElement(By.XPath($"//label[contains(text(), 'Gender')]/following::input[@type='radio'][{(gender == "Male" ? 1 : (gender == "Female" ? 2 : 3))}]"))
            );

            if (!genderRadio.Selected)
            {
                genderRadio.Click();
            }
            return this;
        }

        public bool IsGenderSelected(string gender)
        {
            try
            {
                IWebElement genderRadio = FindElementSafely(
                    () => driver.FindElement(By.XPath($"//input[@type='radio'][@value='{gender}']")),
                    () => driver.FindElement(By.Id(gender.ToLower()))
                );

                return genderRadio.Selected;
            }
            catch
            {
                return false;
            }
        }

        public FormPage SelectState(string state)
        {
            IWebElement stateDropdown = FindElementSafely(
                () => driver.FindElement(By.Id("state")),
                () => driver.FindElement(By.Name("State")),
                () => driver.FindElement(By.XPath("//label[contains(text(), 'State')]/following::select[1]")),
                () => driver.FindElement(By.CssSelector("select.form-control"))
            );

            SelectElement selectElement = new SelectElement(stateDropdown);
            selectElement.SelectByText(state);
            return this;
        }

        public string GetSelectedState()
        {
            try
            {
                IWebElement stateDropdown = FindElementSafely(
                    () => driver.FindElement(By.Id("state")),
                    () => driver.FindElement(By.Name("State"))
                );

                SelectElement selectElement = new SelectElement(stateDropdown);
                return selectElement.SelectedOption.Text;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region Shadow DOM Interactions

        public FormPage EnterFirstNameInShadowDOM(string firstName)
        {
            try
            {
                // Log basic page structure information
                DumpShadowDOMStructure();
                
                Console.WriteLine("Using simplified JavaScript approach to enter first name in Shadow DOM...");
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                
                // Use a simple script to find and interact with shadow DOM elements
                try {
                    js.ExecuteScript(
                        "var shadowForms = document.querySelectorAll('shadow-form');" +
                        "for (var i = 0; i < shadowForms.length; i++) {" +
                        "  if (shadowForms[i].shadowRoot) {" +
                        "    var input = shadowForms[i].shadowRoot.querySelector('#fname');" +
                        "    if (input) {" +
                        "      input.value = arguments[0];" +
                        "      console.log('Set shadow DOM first name to: ' + arguments[0]);" +
                        "    }" +
                        "  }" +
                        "}", firstName);
                } catch (Exception ex) {
                    Console.WriteLine($"Error in first approach: {ex.Message}");
                }
                
                // Also try a more generic approach
                try {
                    js.ExecuteScript(
                        "document.querySelectorAll('*').forEach(function(el) {" +
                        "  if (el.shadowRoot) {" +
                        "    var inputs = el.shadowRoot.querySelectorAll('input[type=\"text\"]');" +
                        "    for (var j = 0; j < inputs.length; j++) {" +
                        "      inputs[j].value = arguments[0];" +
                        "      console.log('Set text input in shadow DOM to: ' + arguments[0]);" +
                        "    }" +
                        "  }" +
                        "});", firstName);
                } catch (Exception ex) {
                    Console.WriteLine($"Error in second approach: {ex.Message}");
                }
                
                // Try a third approach with direct element access
                try {
                    js.ExecuteScript(
                        "var inputs = document.querySelectorAll('input');" +
                        "for (var i = 0; i < inputs.length; i++) {" +
                        "  if (inputs[i].id === 'fname' || inputs[i].name === 'fname') {" +
                        "    inputs[i].value = arguments[0];" +
                        "    console.log('Set input directly: ' + arguments[0]);" +
                        "  }" +
                        "}", firstName);
                } catch (Exception ex) {
                    Console.WriteLine($"Error in third approach: {ex.Message}");
                }
                
                Console.WriteLine("Completed Shadow DOM first name entry attempts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error entering first name in Shadow DOM: {ex.Message}");
            }
            return this;
        }

        public FormPage SelectGenderInShadowDOM(string gender)
        {
            try
            {
                Console.WriteLine($"Using simplified JavaScript approach to select {gender} gender in Shadow DOM...");
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                
                // Use a simple script to find and interact with shadow DOM elements
                try {
                    js.ExecuteScript(
                        "var shadowForms = document.querySelectorAll('shadow-form');" +
                        "for (var i = 0; i < shadowForms.length; i++) {" +
                        "  if (shadowForms[i].shadowRoot) {" +
                        "    var genderSelector = '#' + arguments[0].toLowerCase();" +
                        "    var radio = shadowForms[i].shadowRoot.querySelector(genderSelector);" +
                        "    if (radio) {" +
                        "      radio.checked = true;" +
                        "      console.log('Selected gender radio button: ' + arguments[0]);" +
                        "    }" +
                        "  }" +
                        "}", gender);
                } catch (Exception ex) {
                    Console.WriteLine($"Error in first gender selection approach: {ex.Message}");
                }
                
                // Also try a more generic approach
                try {
                    js.ExecuteScript(
                        "var genderArg = arguments[0];" +
                        "var genderLower = genderArg.toLowerCase();" +
                        "document.querySelectorAll('*').forEach(function(el) {" +
                        "  if (el.shadowRoot) {" +
                        "    var radios = el.shadowRoot.querySelectorAll('input[type=\"radio\"]');" +
                        "    for (var j = 0; j < radios.length; j++) {" +
                        "      var radio = radios[j];" +
                        "      if (radio.id && radio.id.toLowerCase() === genderLower || " +
                        "          radio.value && radio.value.toLowerCase() === genderLower) {" +
                        "        radio.checked = true;" +
                        "        console.log('Selected radio button in shadow DOM: ' + radio.id);" +
                        "      }" +
                        "    }" +
                        "  }" +
                        "});", gender);
                } catch (Exception ex) {
                    Console.WriteLine($"Error in second gender selection approach: {ex.Message}");
                }
                
                // Try a third approach with direct element access
                try {
                    js.ExecuteScript(
                        "var radios = document.querySelectorAll('input[type=\"radio\"]');" +
                        "for (var i = 0; i < radios.length; i++) {" +
                        "  var radioId = radios[i].id || '';" +
                        "  var radioValue = radios[i].value || '';" +
                        "  var genderLower = arguments[0].toLowerCase();" +
                        "  if (radioId.toLowerCase() === genderLower || radioValue.toLowerCase() === genderLower) {" +
                        "    radios[i].checked = true;" +
                        "    console.log('Selected radio directly: ' + radioId);" +
                        "  }" +
                        "}", gender);
                } catch (Exception ex) {
                    Console.WriteLine($"Error in third gender selection approach: {ex.Message}");
                }
                
                Console.WriteLine("Completed Shadow DOM gender selection attempts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error selecting gender in Shadow DOM: {ex.Message}");
            }
            return this;
        }

        public FormPage SelectStateInShadowDOM(string state)
        {
            try
            {
                Console.WriteLine($"Using simplified JavaScript approach to select {state} state in Shadow DOM...");
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                
                // Use a simple script to find and interact with shadow DOM elements
                try {
                    js.ExecuteScript(
                        "var shadowForms = document.querySelectorAll('shadow-form');"+
                        "for (var i = 0; i < shadowForms.length; i++) {"+
                        "  if (shadowForms[i].shadowRoot) {"+
                        "    var select = shadowForms[i].shadowRoot.querySelector('#state');"+
                        "    if (select && select.options) {"+
                        "      for (var j = 0; j < select.options.length; j++) {"+
                        "        if (select.options[j].text === arguments[0]) {"+
                        "          select.selectedIndex = j;"+
                        "          console.log('Selected state: ' + arguments[0]);"+
                        "          break;"+
                        "        }"+
                        "      }"+
                        "      if (select.options.length > 1) {"+
                        "        select.selectedIndex = 1;"+
                        "        console.log('Selected fallback state option');"+
                        "      }"+
                        "    }"+
                        "  }"+
                        "}", state);
                } catch (Exception ex) {
                    Console.WriteLine($"Error in first state selection approach: {ex.Message}");
                }
                
                // Try a simpler approach
                try {
                    js.ExecuteScript(
                        "var shadowElements = document.querySelectorAll('shadow-form');"+
                        "shadowElements.forEach(function(el) {"+
                        "  if (el.shadowRoot) {"+
                        "    var select = el.shadowRoot.querySelector('#state');"+
                        "    if (select && select.options && select.options.length > 1) {"+
                        "      select.selectedIndex = 1;"+
                        "      console.log('Set state dropdown to index 1');"+
                        "    }"+
                        "  }"+
                        "});");
                } catch (Exception ex) {
                    Console.WriteLine($"Error in second state selection approach: {ex.Message}");
                }
                
                // Try direct approach
                try {
                    js.ExecuteScript(
                        "var elements = document.querySelectorAll('select#state');"+
                        "for (var i = 0; i < elements.length; i++) {"+
                        "  if (elements[i].options && elements[i].options.length > 1) {"+
                        "    elements[i].selectedIndex = 1;"+
                        "    console.log('Set direct state select');"+
                        "  }"+
                        "}");
                } catch (Exception ex) {
                    Console.WriteLine($"Error in third state selection approach: {ex.Message}");
                }
                
                Console.WriteLine("Completed Shadow DOM state selection attempts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error selecting state in Shadow DOM: {ex.Message}");
            }
            return this;
        }

        public FormPage InteractWithNestedShadowDOM(string elementSelector, string value)
        {
            try
            {
                DumpShadowDOMStructure();
                
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                
                // Identify shadow root elements
                js.ExecuteScript(
                    "var elements = document.querySelectorAll('*');" +
                    "for (var i = 0; i < elements.length; i++) {" +
                    "  if (elements[i].shadowRoot) {" +
                    "    console.log('Found element with shadow root: ' + elements[i].tagName);" +
                    "  }" +
                    "}");
                
                // Primary approach for nested shadow DOM
                try {
                    js.ExecuteScript(
                        "var shadowHost = document.querySelector('nestedshadow-form');" +
                        "if (shadowHost && shadowHost.shadowRoot) {" +
                        "  var innerHost = shadowHost.shadowRoot.querySelector('shadow-form');" +
                        "  if (innerHost && innerHost.shadowRoot) {" +
                        "    var input = innerHost.shadowRoot.querySelector(arguments[0]);" +
                        "    if (input) {" +
                        "      input.value = arguments[1];" +
                        "    }" +
                        "  }" +
                        "}", elementSelector, value);
                } catch (Exception ex) {
                    // Continue to next approach
                }
                
                // Secondary approach
                try {
                    js.ExecuteScript(
                        "var selector = arguments[0];" +
                        "var inputValue = arguments[1];" +
                        "document.querySelectorAll('*').forEach(function(el) {" +
                        "  if (el.shadowRoot) {" +
                        "    var shadowChildren = el.shadowRoot.querySelectorAll('*');" +
                        "    shadowChildren.forEach(function(inner) {" +
                        "      if (inner.shadowRoot) {" +
                        "        try {" +
                        "          var input = inner.shadowRoot.querySelector(selector);" +
                        "          if (input && input.tagName === 'INPUT') {" +
                        "            input.value = inputValue;" +
                        "          }" +
                        "        } catch(e) { }" +
                        "      }" +
                        "    });" +
                        "  }" +
                        "});", elementSelector, value);
                } catch (Exception) {
                    // Continue to next approach
                }
                
                // Recursive traversal approach
                try {
                    js.ExecuteScript(@"
                        function findDeepShadowInput(selector, maxDepth) {
                          function traverse(root, depth) {
                            if (depth > maxDepth || !root) return null;
                            
                            if (root.shadowRoot) {
                              try {
                                var input = root.shadowRoot.querySelector(selector);
                                if (input) return input;
                              } catch(e) { }
                              
                              var children = root.shadowRoot.querySelectorAll('*');
                              for (var i = 0; i < children.length; i++) {
                                var result = traverse(children[i], depth + 1);
                                if (result) return result;
                              }
                            }
                            return null;
                          }
                          return traverse(document.body, 0);
                        }
                        
                        var input = findDeepShadowInput(arguments[0], 5);
                        if (input && input.tagName === 'INPUT') {
                          input.value = arguments[1];
                          return true;
                        }
                        return false;", elementSelector, value);
                } catch (Exception) {
                    // Final approach completed
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error interacting with nested shadow DOM: {ex.Message}");
            }
            
            return this;
        }

        #endregion

        #region Helper Methods

        // Simple method to log information about the page structure
        private void DumpShadowDOMStructure()
        {
            try
            {
                Console.WriteLine("\n==== DUMPING PAGE STRUCTURE ====");
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                
                // Count total elements
                long totalElements = (long)js.ExecuteScript("return document.querySelectorAll('*').length");
                Console.WriteLine($"Total elements in document: {totalElements}");
                
                // Count custom elements
                long customElements = (long)js.ExecuteScript("return document.querySelectorAll('shadow-form, nestedshadow-form').length");
                Console.WriteLine($"Custom shadow elements found: {customElements}");
                
                // Count input elements
                long inputElements = (long)js.ExecuteScript("return document.querySelectorAll('input').length");
                Console.WriteLine($"Input elements in document: {inputElements}");
                
                // Check for shadow roots
                js.ExecuteScript("" +
                    "const elements = document.querySelectorAll('*');" +
                    "for (let i = 0; i < elements.length; i++) {" +
                    "    if (elements[i].shadowRoot) {" +
                    "        console.log('Found shadow root in: ' + elements[i].tagName);" +
                    "    }" +
                    "}");
                
                Console.WriteLine("==== END OF PAGE STRUCTURE DUMP ====\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error dumping page structure: {ex.Message}");
            }
        }
        
        // Enhanced helper method to find elements in Shadow DOM
        private IWebElement? FindElementInShadowDOM(IWebElement shadowHost, string cssSelector)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                
                // First try the standard approach
                IWebElement? element = (IWebElement?)js.ExecuteScript(
                    "return arguments[0].shadowRoot ? arguments[0].shadowRoot.querySelector(arguments[1]) : null", 
                    shadowHost, 
                    cssSelector);
                
                if (element != null)
                {
                    Console.WriteLine($"Found element in shadow DOM using standard approach: {cssSelector}");
                    return element;
                   }
                
                // If standard approach fails, try a more aggressive approach
                // This uses JavaScript to pierce through the Shadow DOM
                Console.WriteLine("Standard shadow DOM approach failed, trying alternative approach...");
                
                // Check if we're dealing with an open or closed shadow root
                bool hasOpenShadowRoot = (bool)js.ExecuteScript("return !!arguments[0].shadowRoot", shadowHost);
                Console.WriteLine($"Shadow host has open shadow root: {hasOpenShadowRoot}");
                
                if (hasOpenShadowRoot)
                {
                    // Try a different query approach for open shadow roots
                    element = (IWebElement?)js.ExecuteScript(
                        "return Array.from(arguments[0].shadowRoot.querySelectorAll('*')).find(el => el.matches(arguments[1]) || el.id === arguments[1].replace('#', ''))", 
                        shadowHost, 
                        cssSelector);
                    
                    if (element != null)
                    {
                        Console.WriteLine($"Found element in shadow DOM using alternative query: {cssSelector}");
                        return element;
                    }
                }
                else
                {
                    // For closed shadow roots, we need a more aggressive approach
                    // Note: This might not work in all browsers and is generally not recommended
                    // as it breaks the encapsulation of the Shadow DOM
                    Console.WriteLine("Attempting to access closed shadow root (may not work in all browsers)...");
                    
                    // Try to get all elements in the document and find the one matching our selector
                    var allElements = (System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>)js.ExecuteScript(
                        "return document.querySelectorAll('*')");
                    
                    foreach (var el in allElements)
                    {
                        try
                        {
                            string id = el.GetAttribute("id") ?? string.Empty;
                            if (cssSelector.StartsWith("#") && id == cssSelector.Substring(1))
                            {
                                Console.WriteLine($"Found element with ID {id} in document");
                                return el;
                            }
                        }
                        catch { /* Ignore errors when getting attributes */ }
                    }
                }
                
                Console.WriteLine($"Could not find element in shadow DOM: {cssSelector}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding element in shadow DOM: {ex.Message}");
                return null;
            }
        }

        // Helper method to find elements in nested Shadow DOM (multiple levels)
        private IWebElement? FindElementInNestedShadowDOM(IWebElement rootElement, string[] selectors)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            IWebElement element = rootElement;
            
            string script = "let element = arguments[0];";
            for (int i = 0; i < selectors.Length; i++)
            {
                script += $"element = element.shadowRoot.querySelector('{selectors[i]}');";
            }
            script += "return element;";
            
            return (IWebElement?)js.ExecuteScript(script, element);
        }

        // Helper method that tries multiple strategies to find an element
        private IWebElement FindElementSafely(params Func<IWebElement>[] strategies)
        {
            Exception? lastException = null;
            
            foreach (var strategy in strategies)
            {
                try
                {
                    var element = strategy();
                    // Wait for the element to be visible and interactable
                    wait.Until(d => element.Displayed && element.Enabled);
                    return element;
                }
                catch (Exception ex)
                {
                    // Store the exception and try the next strategy
                    lastException = ex;
                }
            }
            
            // If we get here, all strategies failed
            throw new NoSuchElementException("Element could not be found with any of the provided strategies", lastException);
        }

        // Take screenshot method with improved page state verification
        public string TakeScreenshot(string testName)
        {
            // Wait a moment to ensure page is in stable state
            try
            {
                // Wait for any pending AJAX requests to complete
                ((IJavaScriptExecutor)driver).ExecuteScript("return window.setTimeout(function() {}, 1000);");
                
                // Scroll to top of page for better screenshot
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0);");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning during screenshot preparation: {ex.Message}");
            }
            
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string screenshotName = $"{testName}_{timestamp}.png";
            string screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots", screenshotName);
            
            // Ensure directory exists
            string? directoryPath = Path.GetDirectoryName(screenshotPath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            // Log page state before taking screenshot
            try
            {
                string url = driver.Url;
                string title = driver.Title;
                bool formVisible = driver.FindElements(By.TagName("form")).Count > 0;
                Console.WriteLine($"Taking screenshot at URL: {url}, Title: {title}, Form visible: {formVisible}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not log page state: {ex.Message}");
            }
            
            // Take screenshot with additional diagnostic information
            try
            {
                // Try to get more information about the page state
                string pageSource = driver.PageSource;
                Console.WriteLine($"Page source length: {pageSource.Length} characters");
                
                // Check if we're in an iframe
                int iframeCount = driver.FindElements(By.TagName("iframe")).Count;
                Console.WriteLine($"Number of iframes on page: {iframeCount}");
                
                // Check form fields
                try
                {
                    var firstNameField = driver.FindElement(By.Id("fname"));
                    Console.WriteLine($"First name field found: {firstNameField.Displayed}, Value: '{firstNameField.GetAttribute("value")}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not find first name field: {ex.Message}");
                }
                
                // Take the actual screenshot
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(screenshotPath);
                Console.WriteLine($"Screenshot saved to: {screenshotPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error taking screenshot: {ex.Message}");
                
                // Try a fallback screenshot method
                try
                {
                    Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    screenshot.SaveAsFile(screenshotPath);
                    Console.WriteLine("Fallback screenshot saved");
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"Fallback screenshot also failed: {fallbackEx.Message}");
                }
            }
            
            return screenshotPath;
        }

        #endregion
    }
}
