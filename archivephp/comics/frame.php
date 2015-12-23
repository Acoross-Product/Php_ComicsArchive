<!DOCTYPE html>
<meta charset="utf-8" />
<?php
session_start();
if(!isset($_SESSION['user_id']) || !isset($_SESSION['user_name'])) {
	echo "<meta http-equiv='refresh' content='0;url=login.php'>";
	exit;
}
$user_id = $_SESSION['user_id'];
$user_name = $_SESSION['user_name'];
echo "<p>안녕하세요. $user_name($user_id)님</p>";
echo "<p><a href='logout.php'>로그아웃</a></p>";
?>

<?php
$order = 1;
$from = 0;
$title = null;

if(is_numeric($_POST['order']))
{
	$order = $_POST['order'];
}
if (is_numeric($_POST['from']))
{
	$from = $_POST['from'];
}
if (isset($_POST['title']))
{
	$title = $_POST['title'];
}

if(is_numeric($_GET['order']))
{
	$order = $_GET["order"];
}
if (is_numeric($_GET['from']))
{
	$from = $_GET["from"];
}
if (isset($_GET['title']))
{
	$title = $_GET['title'];
}

$from = max($from, 0);
?>

<form method='post' action='index.php'>
<table>
<tr>
	<td>오더</td>
	<td><input type='text' name='order' tabindex='1' value='<?= $order ?>'/></td>
	<td rowspan='1'><input type='submit' tabindex='3' value='검색' style='height:25px'/></td>
</tr>
<tr>
	<td>from</td>
	<td><input type='text' name='from' tabindex='1' value='<?= $from ?>'/></td>
</tr>
<tr>
	<td>제목</td>
	<td><input type='text' name='title' tabindex='1' value='<?= $title ?>'/></td>
</tr>
</table>
</form>

<?php
function prevnext($order, $from, $title)
{
	echo "<a href='index.php?order=".$order."&from=".($from-10)."&title=".$title."'>이전</a>, ";
	echo "<a href='index.php?order=".$order."&from=".($from+10)."&title=".$title."'>다음</a><br/>";
}
?>