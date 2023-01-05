# Project Codebase Overview

Installation:
1. Download the MSIX package
2. Install the certificate to the "Trusted Root Certification Authorities" store like this:
    - Right click on the .msix file
    - Click "properties"
    - Go to the "Digital Signatures" tab
    - Select the certificate
    - Click "Details"
    - Click "View Certificate"
    - Click "Install Certificate"
    - Select "Local Machine"
    - Select "Place all certificates in the following store"
    - Click "Browse"
    - Select "Trusted Root Certification Authorities"
    - Click ok -> next -> finish. 
3. Double click the MSIX package and click "Install" in the prompt.

4. If the .NET 6.0 runtime environment is not installed on your pc you will be prompted to install it.
    Download and install it to run the desktop app. Download it from here:
    https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime?cid=getdotnetcore 



For developer:
msix package is exported by 
- right click on solution
- select "Package and publish" -> "create app packages
- select "sideloading" and next
- "yes use certificate"
- next -> next -> create


