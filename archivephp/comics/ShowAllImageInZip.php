<?php
header('Content-Type: text/html; charset=utf-8');

include_once('DB/imageSizer.php');

function ShowAllImageInZip($path, $filename)
{
    ignore_user_abort(true);
	set_time_limit(0); // disable the time limit for this script
	
	$dl_file = $filename;
	$fullPath = $path.$dl_file;
				
    $path_parts = pathinfo($fullPath);
    $ext = strtolower($path_parts["extension"]);
    
    switch( $ext ) 
    { 
        case "zip": break;
        default: exit; 
    }
    //echo "this is zip ok <br/>";
        
    $zip = new ZipArchive;
    if ($zip->open($fullPath) === true) 
    {
        for($i = 0; $i < $zip->numFiles; $i++) 
        {
            //$zip->extractTo('path/to/extraction/', array($zip->getNameIndex($i)));
            // here you can run a custom function for the particular extracted file
            $cont_name = $zip->getNameIndex($i);
            $cont_path_parts = pathinfo($cont_name);
            $cont_ext = strtolower($cont_path_parts["extension"]);
            
            if ($cont_ext == "jpg")
            {
                $contents = '';
                //echo $cont_name."<br/>";
                $fp = $zip->getStream($cont_name);
                if(!$fp) exit("failed\n");
                
                //echo "get stream...<br/>";
                
                while (!feof($fp)) {
                    $contents .= fread($fp, 2);
                }
                fclose($fp);
				
                $b64 = base64_encode($contents);
				
				list($w, $h) = getimagesizefromstring($contents);
				//echo $w." ".$h."<br/>";
				list($width, $height) = fixWidth(600, $w, $h);
				//echo $width." ".$height."<br/>";
                echo "<img src='data:image/jpeg;base64,$b64' width='".$width."' height='".$height."'><br/>";
            }
        }
                        
        $zip->close();
    }
    else
    {
        echo "fail open file ".$filename."<br/>";
    }
}

$path = $_GET['path'];
$filename = $_GET['filename'];
ShowAllImageInZip($path, $filename);

?>