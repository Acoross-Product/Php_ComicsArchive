<?php
header('Content-Type: text/html; charset=utf-8');

function mb_basename($path) { return end(explode('/',$path)); } 
function utf2euc($str) { return iconv("UTF-8","cp949//IGNORE", $str); }
function is_ie() {
	if(!isset($_SERVER['HTTP_USER_AGENT']))return false;
	if(strpos($_SERVER['HTTP_USER_AGENT'], 'MSIE') !== false) return true; // IE8
	if(strpos($_SERVER['HTTP_USER_AGENT'], 'Windows NT 6.1') !== false) return true; // IE11
	if(strpos($_SERVER['HTTP_USER_AGENT'], 'Windows') !== false) return true; // edge
	return false;
}
?>

<?php
function download($path, $filename)
{
	ignore_user_abort(true);
	set_time_limit(0); // disable the time limit for this script
	
	//$path = "/Extern/Media/2. 만화/";
	//$path = "/absolute_path_to_your_files/"; // change the path to fit your websites document structure
	$dl_file = $filename;
	$fullPath = $path.$dl_file;
		
	if ($fd = fopen ($fullPath, "r")) {
		$fsize = filesize($fullPath);
				
		$path_parts = pathinfo($fullPath);
		$ext = strtolower($path_parts["extension"]);
		
		switch( $ext ) 
		{ 
			case "zip": $ctype="application/zip"; break;
			case "rar": $ctype="application/rar"; break;
			default: $ctype="application/force-download"; 
		}

		header("Pragma: public"); 
		header("Expires: 0"); 
		header("Cache-Control: must-revalidate, post-check=0, pre-check=0"); 
		header("Cache-Control: public"); 
		header("Content-Description: File Transfer"); 
		header("Content-Type: $ctype"); 

		if (is_ie() )
		{
			$dl_file = utf2euc($dl_file);
		}
		$header="Content-Disposition: attachment; filename='".$dl_file."';"; 
		header($header ); 
		header("Content-Transfer-Encoding: binary"); 
		
		header("Content-length: $fsize");
		
		ob_end_flush();
		while(!feof($fd)) {
			$buffer = fread($fd, 2048);
			echo $buffer;
		}
	}
	else
	{
		echo "fail ".$fullPath."<br/>";
		exit;
	}
	fclose ($fd);
	exit;
}

download($_GET['path'], $_GET['filename']);
?>
