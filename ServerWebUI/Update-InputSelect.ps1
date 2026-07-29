$root = Get-Location

Get-ChildItem $root -Recurse -Filter *.razor | ForEach-Object {

    $content = Get-Content $_.FullName -Raw

    $updated = [regex]::Replace(
        $content,
        '<div class="form-box-group">(.*?)</div>',
        {
            param($m)

            $block = $m.Value

            if ($block -match '<InputSelect\b' -and $block -notmatch 'static-label')
            {
                $block = $block.Replace(
                    '<div class="form-box-group">',
                    '<div class="form-box-group static-label">'
                )
            }

            return $block
        },
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    )

    if ($updated -ne $content)
    {
        Set-Content $_.FullName $updated -Encoding UTF8
        Write-Host "Updated: $($_.Name)"
    }
}

Write-Host ""
Write-Host "Completed."