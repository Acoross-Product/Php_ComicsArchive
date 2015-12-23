<?php
ini_set("display_errors","1");
ERROR_REPORTING( E_ALL | E_STRICT );
include_once('UniversalConnect.php');

class ShowComicsNew
{
	private $test;
	public function __construct()
	{
		try
		{
			$this->test = UniversalConnect::doConnect();
            mysqli_set_charset($this->test, 'utf8'); 
			//echo "You're hooked up, Ace!<br/>";
		}
		catch(Exception $e)
		{
			echo $e->getMessage();
		}
	}
    
    public function Bind($order, $from, $title)
    {
		$order_str = "desc";
		if ($order > 0)
		{
			$order_str = "asc";
		}
		//$from = $GLOBALS['from'];
		
		//echo "try query<br/><br/>";
		$comic_new_query = "SELECT comic_id, title, title_img, filepath, filename, filesize FROM comics_new order by comic_id ".$order_str." LIMIT ".$from.", 10";
		if (!empty($title))
		{
			echo "find by title<br/>";
			$comic_new_query = "SELECT comic_id, title, title_img, filepath, filename, filesize FROM comics_new WHERE title like '%".$title."%' order by comic_id ".$order_str." LIMIT ".$from.", 10";
		}
		
        if ($result = $this->test->query($comic_new_query))
        {
			echo '<table border="1">';
			while($obj = $result->fetch_object())
            {
				$fix_height = 350;
				echo '<tr><th>';
				printf("%d %s", $obj->comic_id, $obj->title);
				echo "<br/>";
				echo ($obj->filesize>>20)."mb";
				echo '</th></tr>';
				
				echo '<tr><th>';
				$b64 = base64_encode($obj->title_img);				
				list($width, $height) = getimagesizefromstring($obj->title_img);
				$ratio = $width / $height;
				$width = $fix_height * $ratio;
				$width = min($width, 500);
				
                echo "<a href='filedownload.php?path=".$obj->filepath."&filename=".$obj->filename."'><img src='data:image/jpeg;base64, $b64' width='".$width."' height='".$fix_height."'></a>";
				echo '</th></tr>';
            }
			
            $result->close();
			echo '</table>';
        }
    }
    
    function __destruct()
    {
        //echo "<br/>dtor<br/>";
        $this->test->close();
    }
}
?>
