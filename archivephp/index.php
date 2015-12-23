<!DOCTYPE html>
<html>
<body>

<?php
echo 'Current PHP version: ' . phpversion();
?>

<form action="upload.php" method="post" enctype="multipart/form-data">
    Select image to upload:
    <input type="file" name="fileToUpload" id="fileToUpload">
    <input type="submit" value="Upload Image" name="submit">
</form>

<?php
echo 'haha';
echo 'hoho';
$link = mysql_connect('127.0.0.1:3306', 'acoross', 'emfdjdhwlak');
echo 'hoho';
if (!$link) {
    echo 'die';
    die('Could not connect: ' . mysql_error());
}
echo 'Connected successfully';
mysql_close($link);
?>

</body>
</html>