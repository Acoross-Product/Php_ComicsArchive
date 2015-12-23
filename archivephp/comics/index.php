<?php
header('Content-Type: text/html; charset=utf-8');
?>

<?php
include_once('frame.php');
prevnext($order, $from, $title);
?>

<?php
ini_set("display_errors","1");
ERROR_REPORTING( E_ALL | E_STRICT );
//include_once('UniversalConnect.php');
include_once('DB/ShowComicsNew.php');

$worker = new ShowComicsNew();
if ($worker)
{
    $worker->Bind($order, $from, $title);
}
?>

<?php
prevnext($order, $from, $title);
echo "<br/><br/>";
?>
