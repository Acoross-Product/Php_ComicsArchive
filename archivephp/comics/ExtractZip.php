<?php
class ExtractZip
{
	public static function Extract($filepath)
	{
		$zip = new ZipArchive;
        $res = $zip->open(filepath);
        if ($res === TRUE){
            $zip->
        }
	}
}
?>