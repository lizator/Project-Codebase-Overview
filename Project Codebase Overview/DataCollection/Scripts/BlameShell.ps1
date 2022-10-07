$FilePath = $args[0]

$Pattern = @('(?<Key>[\^]?[a-f0-9]+) .*\([<](?<Email>.+)[>][ ]+(?<DateString>[0-9]{4}-[0-9]{2}-[0-9]{2}) [0-9]{2}:[0-9]{2}:[0-9]{2} [\-\+]{0,1}[0-9]{4}[ ]+[0-9]+\).*')

$Blame = foreach ($line in git blame -e $FilePath) {
    if ($line -match $Pattern) {
        $Matches.Remove(0)
        $Matches.Key + '|' + $Matches.Email + '|' + $Matches.DateString
    } 
} 

$Blame | Group-Object -NoElement


