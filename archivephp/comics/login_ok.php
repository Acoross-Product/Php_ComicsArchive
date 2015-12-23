<?php
if(!isset($_POST['user_id']) || !isset($_POST['user_pw'])) exit;
$user_id = $_POST['user_id'];
$user_pw = $_POST['user_pw'];
$members = array('acoross'=>array('pw'=>'emfdjdhwlak', 'name'=>'owner'),
				'guest'=>array('pw'=>'guest123', 'name'=>'손님'));
 
if(!isset($members[$user_id])) {
		echo "11 <br/>";
        echo "<script>alert('아이디 또는 패스워드가 잘못되었습니다.');history.back();</script>";
        exit;
}
if($members[$user_id]['pw'] != $user_pw) {
		echo "22 <br/>";
        echo "<script>alert('아이디 또는 패스워드가 잘못되었습니다.');history.back();</script>";
        exit;
}
session_start();
$_SESSION['user_id'] = $user_id;
$_SESSION['user_name'] = $members[$user_id]['name'];
?>
<meta http-equiv='refresh' content='0;url=index.php'>