namespace J4JMapLibrary;

public class SignedCredential : Credential
{
    public string Signature { get; set; } = string.Empty;

    public override bool IsValid => base.IsValid && !string.IsNullOrEmpty( Signature );
}
