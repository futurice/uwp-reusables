# UWP Reusables

##Controls

###ValidatingTextBox
A text box that validates all input, and displays error messages if any of the validation functions fail.

Can be used in XAML:

      <vtb:ValidatingTextBox PlaceholderText="No dots or exclamations allowed.">
                <vtb:ValidationPair ErrorMessage="No dots allowed." ValidationFunction="{x:Bind NoDotsFunction}"/>
                <vtb:ValidationPair ErrorMessage="Exclamation marks not allowed either >:|" ValidationFunction="{x:Bind NoExclamationsFunction}"/>
     </vtb:ValidatingTextBox>

and the validation functions bound to are simply defined in code-behind as follows:
 	
	public Func<string, bool> NoDotsFunction { get; set; } = NoDotsFunctionImpl;
    public Func<string, bool> NoExclamationsFunction { get; set; } = NoExclamationsImpl;

    private static bool NoDotsFunctionImpl(string input)
    {
        if (input.Contains("."))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private static bool NoExclamationsImpl(string arg)
    {
        if (arg.Contains("!"))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

Or defined in code-behind:
    
    string locallyScopedString = "closures work just fine";
    ValidatingTextBox codeBehindBox = new ValidatingTextBox();
    codeBehindBox.ValidationPairs.Add(new ValidationPair
    {
        ValidationFunction = s => s.Contains("@"),
        ErrorMessage = $"Gotta have at least one @ here. And {locallyScopedString}!",                
    });          