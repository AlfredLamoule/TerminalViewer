using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows;

namespace TerminalViewer
{
    class ProcessMapping
    {
        private static readonly int RAYON_TERRE = 6371000;

        private int resize;
	    private int marge;
	    private int translationX;
	    private int translationY;
        
	    private double CoefA;
        private double CoefB;
        private double CoefC;
        private double CoefD;
        private double CoefE;
        private double CoefF;

        public ProcessMapping(int translationX, int translationY, int resize, int marge)
        {
            this.translationX = translationX;
            this.translationY = translationY;
            this.resize = resize;
            this.marge = marge;
        }

        public void CalculateMargin()
        {
            double distPixel = this.GetDistance(ConvertGPSToPixel(0, 0), ConvertGPSToPixel(0, 1));

	        marge = (int)(marge / distPixel*Math.Sqrt(2.0));
        }

        // MATRICES
        public void BuildMapMatrix(double[] matrixGPS, double[] matrixPixelX, double[] matrixPixelY) {
	        // Utilisation de la formule de Cramer
	        double denum;

	        double[] num = new double[6];
	        double[] base3X3det = new double[9];
	        for(int i = 0 ; i < 6 ; i ++)
		        base3X3det[i] = matrixGPS[i];
	        //vector<double> num;
	        //vector<double> base3X3det(matrixGps); // Initialisée à matrixGps
	        // Construire de la matrix de base de GPS
	        //
	        //  | GPSX1  GPSX2  GPSX3  |    même determinant	|  GPSX1  GPSY1  1  |      [0]  [3]  [6]     
	        //  | GPSY1  GPSY2  GPSY3  |  <-----------------.  |  GPSX2  GPSY2  1  |      [1]  [4]  [7]    
	        //  |   1      1      1    |					    |  GPSX3  GPSY3  1  |	   [2]  [5]  [8]
	
	        base3X3det[6] = 1.0;
	        base3X3det[7] = 1.0;
	        base3X3det[8] = 1.0;

	        double[] tmp_3X3det = new double[9];
	        base3X3det.CopyTo(tmp_3X3det, 0);
 
	        // Calcul du dénumérateur commun 
	        denum=Resolve3X3Det(base3X3det);

	        int nb = 0;
	        // Calcul des numérateurs pour les coeifs a,b,c 
	        for(int i=0;i<=6;i=i+3) {
		        for(int j=0;j!=3;j++) {
			        tmp_3X3det[i+j]=matrixPixelX[j];
		        }
		        num[nb] = Resolve3X3Det(tmp_3X3det);
		        nb ++;
		        base3X3det.CopyTo(tmp_3X3det, 0);
	        }

	        // Calcul des numérateurs pour les coefs d,e,f
	        for(int i=0;i<=6;i=i+3) {
		        for(int j=0;j!=3;j++) {
			        tmp_3X3det[i+j]=matrixPixelY[j];
		        }
		        num[nb] = Resolve3X3Det(tmp_3X3det);
		        nb ++;
		        base3X3det.CopyTo(tmp_3X3det, 0);
	        }
	        // Calcul des coefficients
	        this.CoefA=num[0]/denum;	
	        this.CoefB=num[1]/denum;	
	        this.CoefC=num[2]/denum;
	        this.CoefD=num[3]/denum;	
	        this.CoefE=num[4]/denum;	
	        this.CoefF=num[5]/denum;
        }

        // TransX ==> TranslationX fichier config - 80
        // TransY ==> TranslationY fichier config - 35
        public void BuildMatrixTerminal(String terminal) {
	        double[] GPS  = new double[6];
	        double[] PixX = new double[3];
	        double[] PixY = new double[3];

	        if(String.Compare(terminal, "TNORD") == 0) {
		        const int coef_translation = 2;
		
		        GPS[0]=0.162226;
                GPS[1]=0.167809;
                GPS[2]=0.183937;
                GPS[3]=49.475416;
                GPS[4]=49.480399;
                GPS[5]=49.486765;
		        // Resize(chercher le vecteur à l'origine, garder la référence, changer les deux autres vecteurs)
		        // Translation( faire la translation des trois points)
		        PixX[0]=22+translationX*coef_translation;PixX[1]=(298-22)*resize/100+22+translationX*coef_translation;PixX[2]=(812-22)*resize/100+22+translationX*coef_translation;
		        PixY[0]=188+60+translationY*coef_translation;PixY[1]=188+(48-60)*resize/100+60+translationY*coef_translation;PixY[2]=188+(230-60)*resize/100+60+translationY*coef_translation;
		        // 188 : différence entre le nouveau picturebox nouveau et le vieux
	        } else {
		        const int coef_translation = 2;
		
		        GPS[0]=0.165226;GPS[1]=0.164121;GPS[2]=0.179694;GPS[3]=49.461980;GPS[4]=49.458616;GPS[5]=49.456443;
		        // Resize(chercher le vecteur à l'origine, garder la référence, changer les deux autres vecteurs)
		        // Translation( faire la translation des trois points)
		        PixX[0]=70+translationX*coef_translation;PixX[1]=(70-70)*resize/100+70+translationX*coef_translation;PixX[2]=(756-70)*resize/100+70+translationX*coef_translation;
		        PixY[0]=188+30+translationY*coef_translation;PixY[1]=188+(235-30)*resize/100+30+translationY*coef_translation;PixY[2]=188+(235-30)*resize/100+30+translationY*coef_translation;
		        // 188 : différence entre le nouveau picturebox nouveau et le vieux
	        }

	        BuildMapMatrix(GPS,PixX,PixY);
        }

        public void BuildMatrixTerminal(String terminal, double ratio, int offsetX, int offsetY) {
	        double[] GPS  = new double[6];
	        double[] PixX = new double[3];
	        double[] PixY = new double[3];

            // Resize = 159
            // 0.8 < Ratio < 10.5   (pas 0.1)
            // 127.2 < tmp_Resize < 1669.5

	        int tmp_Resize = (int)(resize * ratio);

	        if(String.Compare(terminal, "TNORD") == 0) {
		        GPS[0]=0.162226;GPS[1]=0.167809;GPS[2]=0.183937;GPS[3]=49.475416;GPS[4]=49.480399;GPS[5]=49.486765;
		        
		        PixX[0] = offsetX;
                PixX[1] = 276 * tmp_Resize/100 + offsetX;
                PixX[2] = 790 * tmp_Resize/100 + offsetX;

		        PixY[0] = offsetY;
                PixY[1] = -12 * tmp_Resize/100 + offsetY;
                PixY[2] = 170 * tmp_Resize/100 + offsetY;
		        
	        } else {
		        GPS[0]=0.165226;GPS[1]=0.164121;GPS[2]=0.179694;GPS[3]=49.461980;GPS[4]=49.458616;GPS[5]=49.456443;
		        
                PixX[0] = offsetX; 
                PixX[1] = offsetX; 
                PixX[2] = 686 * tmp_Resize / 100 + offsetX;
                
                PixY[0] = offsetY; 
                PixY[1] = 205 * tmp_Resize / 100 + offsetY; 
                PixY[2] = 205 * tmp_Resize / 100 + offsetY;

	        }

	        BuildMapMatrix(GPS,PixX,PixY);
        }

        // Conditions avant appel :
        // - Vérifier que le block existe
        // - Vérifier que le block en question contient plusieurs slots

        // Coordonnées du block ; Latitude centrale du block ; Longitude centrale du block ; Largeur de la pictureBox ; Hauteur de la pictureBox ; Ratio de zoom
        public void BuildMatrixBlock(List<double> blockCoords, double centerLat, double centerLon, int sizeX, int sizeY, double ratio) {
	        // Calcul du centre de la map
	        Point mapCenter = new Point(sizeX/2, sizeY/2);

	        List<double[]> blockCoordsWithMargin = GetBlockCoordsWithMargin(blockCoords);
	        double[] mapCenterGPS = ConvertPixelToGPS(mapCenter);

	        // Etape 1: translation au centre de l'écran
	        double translationLat = mapCenterGPS[0] - centerLat;
	        double translationLon = mapCenterGPS[1] - centerLon;

	        List<double> tranlatedMargeGPS = new List<double>();
	        tranlatedMargeGPS.Add(blockCoordsWithMargin[0][0] + translationLon);
	        tranlatedMargeGPS.Add(blockCoordsWithMargin[0][1] + translationLat);
	        tranlatedMargeGPS.Add(blockCoordsWithMargin[1][0] + translationLon);
	        tranlatedMargeGPS.Add(blockCoordsWithMargin[1][1] + translationLat);

	        // Etape 2: zoom à partir du centroid
	        Point translatedMargePixel1 = ConvertGPSToPixel(tranlatedMargeGPS[0],tranlatedMargeGPS[1]);
	        Point translatedMargePixel2 = ConvertGPSToPixel(tranlatedMargeGPS[2],tranlatedMargeGPS[3]);

	        double dist1 = GetDistance(translatedMargePixel1, mapCenter);
	        double dist2 = GetDistance(translatedMargePixel2, mapCenter);

	        double max_mapSize = sizeX > sizeY ? sizeX / 2 : sizeY / 2;
	        double distResize  = max_mapSize * ratio;

	        List<double> resizedMargePixel = new List<double>();
	        resizedMargePixel.Add((double)mapCenter.X + (double)(translatedMargePixel1.X - mapCenter.X) * distResize/dist1);
	        resizedMargePixel.Add((double)mapCenter.Y + (double)(translatedMargePixel1.Y - mapCenter.Y) * distResize/dist1);
	        resizedMargePixel.Add((double)mapCenter.X + (double)(translatedMargePixel2.X - mapCenter.X) * distResize/dist2);
	        resizedMargePixel.Add((double)mapCenter.Y + (double)(translatedMargePixel2.Y - mapCenter.Y) * distResize/dist2);
	
	        // Etape 3: établi de matrice
	        double[] GPS = new double[6];
	        double[] PixX = new double[3];
	        double[] PixY = new double[3];
	
	        /*Center GPS*/						/*Center Pixel*/
	        GPS[0]=centerLon;					PixX[0]=(double)mapCenter.X;
	        GPS[3]=centerLat;					PixY[0]=(double)mapCenter.Y;
	        /*Point A GPS*/						/*Point A Pixel*/
	        GPS[1]=blockCoordsWithMargin[0][0];	PixX[1]=resizedMargePixel[0];
	        GPS[4]=blockCoordsWithMargin[0][1];	PixY[1]=resizedMargePixel[1];
	        /*Point B GPS*/						/*Point B Pixel*/
	        GPS[2]=blockCoordsWithMargin[1][0];	PixX[2]=resizedMargePixel[2];
	        GPS[5]=blockCoordsWithMargin[1][1];	PixY[2]=resizedMargePixel[3];
	
	        BuildMapMatrix(GPS,PixX,PixY);	

        }

        // Détection de collision entre point et rectangle

        public bool CheckCollision(List<double[]> zoneCoords, double lat, double lon) {
	        List<int> sign = new List<int>();

	        double[] base3X3det = new double[9];
	        for(int i = 0 ; i < 9 ; i ++)
		        base3X3det[i] = 1.0;

	        base3X3det[6] = lon;
	        base3X3det[7] = lat;

	        double[] tmp_3X3det = new double[9];
	        base3X3det.CopyTo(tmp_3X3det, 0);

	        // Calcul des surfaces vectorielles des triangles, et determiner les signes de chacun
	        for(int i = 0 ; i < 3 ; i ++){
		        tmp_3X3det[0] = zoneCoords[i][0];
		        tmp_3X3det[1] = zoneCoords[i][1];
		        tmp_3X3det[3] = zoneCoords[i+1][0];
		        tmp_3X3det[4] = zoneCoords[i+1][1];
		        sign.Add(Resolve3X3Det(tmp_3X3det) > 0 ? 1 : -1);

		        base3X3det.CopyTo(tmp_3X3det, 0);
	        }

	        tmp_3X3det[0] = zoneCoords[3][0];
	        tmp_3X3det[1] = zoneCoords[3][1];
	        tmp_3X3det[3] = zoneCoords[0][0];
	        tmp_3X3det[4] = zoneCoords[0][1];

	        sign.Add(Resolve3X3Det(tmp_3X3det) > 0 ? 1 : -1);
	
	        // Le point est dans la zone si et seulement si les signes sont tous positifs ou négatifs
	
	        int signSum = 0;

	        foreach(int i in sign)
		        signSum += i;

	        if(Math.Abs(signSum) == 4) 
		        return true;
	
	        return false;
        }

        public bool CheckCollision(Point[] points, int x, int y) {
	        List<int> sign = new List<int>();

	        int[] base3X3det = new int[9];
	        for(int i = 0 ; i < 9 ; i ++)
		        base3X3det[i] = 1;

	
	        base3X3det[6] = x;
	        base3X3det[7] = y;

	        int[] tmp_3X3det = new int[9];
	        base3X3det.CopyTo(tmp_3X3det, 0);

	        // Calcul des surfaces vectorielles des triangles, et determiner les signes de chacun
	        for(int i = 0 ; i < 3 ; i ++){
		        tmp_3X3det[0] = (int)points[i].X;
                tmp_3X3det[1] = (int)points[i].Y;
                tmp_3X3det[3] = (int)points[i + 1].X;
                tmp_3X3det[4] = (int)points[i + 1].Y;
		        sign.Add(Resolve3X3Det(tmp_3X3det) > 0 ? 1 : -1);

		        base3X3det.CopyTo(tmp_3X3det, 0);
	        }

            tmp_3X3det[0] = (int)points[3].X;
            tmp_3X3det[1] = (int)points[3].Y;
            tmp_3X3det[3] = (int)points[0].X;
	        tmp_3X3det[4] = (int)points[0].Y;

	        sign.Add(Resolve3X3Det(tmp_3X3det) > 0 ? 1 : -1);
	
	        // Le point est dans la zone si et seulement si les signes sont tous positifs ou négatifs
	
	        int signSum = 0;

	        foreach(int i in sign)
		        signSum += i;

	        if(Math.Abs(signSum) == 4) 
		        return true;
	
	        return false;
        }

        // Détection de collision entre rectangle et rectangle
        public bool CheckCollision(Point[] points, Rect rect)
        {
            foreach (Point point in points)
                if (rect.Contains(point))
                    return true;

            return false;
        }

        // GETTERS

        // 

        // Distance entre deux points en coordonnées GPS
        /*double GetDistance(PointF pointGPS1, PointF pointGPS2) {
	        return (double)Math.Sqrt((double)(pointGPS1.X - pointGPS2.X)*(pointGPS1.X - pointGPS2.X)+(double)(pointGPS1.Y - pointGPS2.Y)*(pointGPS1.Y - pointGPS2.Y));
        }*/

        public double GetDistance(Point pointGPS1, Point pointGPS2) {

		    double distanceAng = 0;

			double radLatA = (Math.PI / 180) * pointGPS1.X; //ptr_NMEAProcess->ReadLatitudeValue();
			double radLongA = (Math.PI / 180) * pointGPS1.Y; //ptr_NMEAProcess->ReadLongitudeValue();

			double radLatB = (Math.PI / 180) * pointGPS2.X; //ptr_NMEAProcess->ReadLatitudeValue();
			double radLongB = (Math.PI / 180) * pointGPS2.Y; //ptr_NMEAProcess->ReadLongitudeValue();

			distanceAng = Math.Acos((Math.Sin(radLatA) * Math.Sin(radLatB)) + (Math.Cos(radLatA) * Math.Cos(radLatB) * Math.Cos(radLongB - radLongA)));

            return distanceAng * RAYON_TERRE;

	        //return (double)Math.Sqrt((double)(pointGPS1.X - pointGPS2.X)*(pointGPS1.X - pointGPS2.X)+(double)(pointGPS1.Y - pointGPS2.Y)*(pointGPS1.Y - pointGPS2.Y));
        }

        // Marge
        List<double[]> GetBlockCoordsWithMargin(List<double> gpsCoords) {
	        List<double[]> tempMarge = new List<double[]>();
	
	        tempMarge.Add( new double[2]{ gpsCoords[0], gpsCoords[1] } );
	        tempMarge.Add( new double[2]{ gpsCoords[2], gpsCoords[3] } );
	        tempMarge.Add( new double[2]{ gpsCoords[4], gpsCoords[5] } );
	        tempMarge.Add( new double[2]{ gpsCoords[6], gpsCoords[7] } );
	
	        return GetBlockCoordsWithMargin(tempMarge);
        }

        List<double[]> GetBlockCoordsWithMargin(List<double[]> gpsCoords) {

            double dist1 = this.GetDistance(new Point((float)gpsCoords[0][0], (float)gpsCoords[0][1]), new Point((float)gpsCoords[1][0], (float)gpsCoords[1][1]));
            double dist2 = this.GetDistance(new Point((float)gpsCoords[0][0], (float)gpsCoords[0][1]), new Point((float)gpsCoords[3][0], (float)gpsCoords[3][1]));

	        double deltaDroiteLon	= (gpsCoords[1][0] - gpsCoords[0][0]) / dist1 * this.marge;
	        double deltaDroiteLat	= (gpsCoords[1][1] - gpsCoords[0][1]) / dist1 * this.marge;
	        double deltaBasLon		= (gpsCoords[3][0] - gpsCoords[0][0]) / dist2 * this.marge;
	        double deltaBasLat		= (gpsCoords[3][1] - gpsCoords[0][1]) / dist2 * this.marge;

	        List<double[]> gpsCoordsWithMargin = new List<double[]>();

	        gpsCoordsWithMargin.Add(new double[2] { gpsCoords[0][0]-deltaDroiteLon-deltaBasLon, gpsCoords[0][1]-deltaDroiteLat-deltaBasLat } );
	        gpsCoordsWithMargin.Add(new double[2] { gpsCoords[1][0]-deltaDroiteLon-deltaBasLon, gpsCoords[1][1]-deltaDroiteLat-deltaBasLat } );
	        gpsCoordsWithMargin.Add(new double[2] { gpsCoords[2][0]-deltaDroiteLon-deltaBasLon, gpsCoords[2][1]-deltaDroiteLat-deltaBasLat } );
	        gpsCoordsWithMargin.Add(new double[2] { gpsCoords[3][0]-deltaDroiteLon-deltaBasLon, gpsCoords[3][1]-deltaDroiteLat-deltaBasLat } );

	        return gpsCoordsWithMargin;
        }

        // CALCULS
        // 1 ==> lat
        // 2 ==> lon
        public double[] ConvertPixelToGPS(Point point) {
            int x = (int)point.X;
            int y = (int)point.Y;

	        double lat,lon;
	        // Système linéaire : 
	        //
	        //    x=A*lon+B*lat+C    .      lon=f(x,y)
	        //    y=D*lon+E*lat+F    .      lat=f(x,y)
	        //
	        lat=(x*CoefD-y*CoefA+CoefF*CoefA-CoefC*CoefD)/(CoefB*CoefD-CoefE*CoefA);
	        lon=(x*CoefE-y*CoefB+CoefF*CoefB-CoefC*CoefE)/(CoefA*CoefE-CoefB*CoefD);
	
	        return new double[2] { lat, lon };
        }

        public Point ConvertGPSToPixel(double lat, double lon) {
	        Point point = new Point();
            double tmp_x = lat * CoefA + lon * CoefB + CoefC;
            double tmp_y = lat * CoefD + lon * CoefE + CoefF;
	        point.X = tmp_x;
	        point.Y = tmp_y;

	        return point;
        }

        double Resolve3X3Det(double[] det) 
        {
	        return det[0]*det[4]*det[8]+det[3]*det[7]*det[2]+det[6]*det[1]*det[5]-det[2]*det[4]*det[6]-det[5]*det[7]*det[0]-det[8]*det[1]*det[3];
        }

        double Resolve3X3Det(int[] det) 
        {
	        return det[0]*det[4]*det[8]+det[3]*det[7]*det[2]+det[6]*det[1]*det[5]-det[2]*det[4]*det[6]-det[5]*det[7]*det[0]-det[8]*det[1]*det[3];
        }

    }
}
