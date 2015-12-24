<?php
function fixHeight($fix_height, $width, $height)
{
	$ratio = $width / $height;
	$width = $fix_height * $ratio;
	return array($width, $fix_height);
}

function fixWidth($fix_width, $width, $height)
{
	$ratio = $height / $width;
	$height = $fix_width * $ratio;
	return array($fix_width, $height);
}

?>