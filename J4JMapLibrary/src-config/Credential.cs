using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JMapLibrary;

public class Credential
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;

    public bool IsValid => !string.IsNullOrEmpty( Name ) && !string.IsNullOrEmpty( Key );
}
