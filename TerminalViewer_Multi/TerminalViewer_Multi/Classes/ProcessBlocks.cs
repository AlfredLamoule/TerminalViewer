using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace TerminalViewer
{
    class ProcessBlocks
    {
        private List<Block> blocksList;

        private double minLat;
        private double minLon;
        private double maxLat;
        private double maxLon;
        
        public ProcessBlocks() {
            this.blocksList = new List<Block>();

            this.maxLat = 0.0;
            this.maxLon = 0.0;
            this.minLat = 999;
            this.minLon = 999;
        }

        // INITIALISATION
        public List<double> LoadBlocksFile(String filename) {
            StreamReader sr;
            try {
                sr = File.OpenText(filename);
            } catch(Exception) {
                return null;
            }

            String blockName = null;
            List<String> tmp_InfosList = null;

            String currLine;
            while((currLine = sr.ReadLine()) != null) {
                int firstdot = currLine.IndexOf('.');
                currLine = currLine.Remove(0, firstdot + 1);

                if(firstdot == 1) {
                    if(tmp_InfosList != null)
                        CreateBlock(blockName, tmp_InfosList);

                    blockName = currLine;
                    tmp_InfosList = new List<String>();
                } else if(firstdot == 2)
                    tmp_InfosList.Add(currLine);
            }

            sr.Close();

            List<double> coords = new List<double>();

            coords.Add(minLat);
	        coords.Add(minLon);
	        coords.Add(maxLat);
	        coords.Add(maxLon);

	        return coords;
        }

        public bool SaveBlockCoords(String terminal, String filename, String dest, Block block)
        {
            String content = "";

            StreamReader sr;
            try
            {
                sr = File.OpenText(filename);
            }
            catch (Exception)
            {
                return false;
            }
            
            String blockName = null;
            int pos = -1;
            
            String currLine;
            while ((currLine = sr.ReadLine()) != null)
            {
                int firstdot = currLine.IndexOf('.');
                
                if (firstdot == 1)
                {
                    pos = -1;

                    blockName = currLine.Remove(0, firstdot + 1);
                    if (blockName.CompareTo(block.Name) == 0)
                        pos = 0;
                    
                }
                else if (firstdot == 2)
                {
                    if (pos > -1)
                        pos++;

                    switch (pos)
                    {
                        // Travées
                        case 2:
                            currLine = String.Format("\t\t.{0}", block.nbTravees);
                            break;
                        // Slots
                        case 3:
                            currLine = String.Format("\t\t.{0}", block.nbSlots);
                            break;
                        case 7:
                            currLine = String.Format("\t\t.{0}", block.coords[0]);
                            break;
                        case 8:
                            currLine = String.Format("\t\t.{0}", block.coords[1]);
                            break;
                        case 9:
                            currLine = String.Format("\t\t.{0}", block.coords[2]);
                            break;
                        case 10:
                            currLine = String.Format("\t\t.{0}", block.coords[3]);
                            break;
                        case 11:
                            currLine = String.Format("\t\t.{0}", block.coords[4]);
                            break;
                        case 12:
                            currLine = String.Format("\t\t.{0}", block.coords[5]);
                            break;
                        case 13:
                            currLine = String.Format("\t\t.{0}", block.coords[6]);
                            break;
                        case 14:
                            currLine = String.Format("\t\t.{0}", block.coords[7]);
                            break;
                    }
                }

                content += String.Format("{0}\r\n", currLine);
            }

            sr.Close();

            File.WriteAllText(dest, content);
            
            return true;
        }

        void CreateBlock(String blockName, List<String> strInfosList) {

            Block block = new Block(blockName);

            // Nombre de travées et de slots par travée
            int nbTravees   = Convert.ToInt32(strInfosList[1]);
            int nbSlots     = Convert.ToInt32(strInfosList[2]);

            // Coordonnées des 4 coins du block
            List<double> coordsList = new List<double>();
            for(int i = 6 ; i <= 13 ; i ++) {
                double coord = Convert.ToDouble(strInfosList[i]);
                coordsList.Add(coord);

                // Longitude
                if(i % 2 == 0) {
                    if(minLon > coord)
                        minLon = coord;
                    if(maxLon < coord)
                        maxLon = coord;
                // Latitude
                } else {
                    if(minLat > coord)
                        minLat = coord;
                    if(maxLat < coord) 
                        maxLat = coord;
                }
            }

            // Calcul du centre
            double[] center = getCenter(coordsList);

            // Validation
            block.setPosition(coordsList);
            block.setCenter(center[0], center[1]);
            block.setSize(nbTravees, nbSlots);

            blocksList.Add(block);
        }


        // GETTERS
        public List<Block> getAllBlocks() {
	        return this.blocksList;
        }

        Block getBlock(String blockname) {
	        foreach(Block block in blocksList)
		        if(String.Compare(block.getName(), blockname) == 0)
			        return block;
	        return null;
        }

        // Position
        public List<double[]> getTraveeCoords(Block block, int numTravee) {
	        List<double> blockCoords = block.getCoords();

	        int nbTravees = block.getNbTravees();
	        numTravee -= block.getFirstTravee();
	
	        double delta1Lon = (blockCoords[6] - blockCoords[0]) / nbTravees;
	        double delta1Lat = (blockCoords[7] - blockCoords[1]) / nbTravees;
	        double delta2Lon = (blockCoords[4] - blockCoords[2]) / nbTravees;
	        double delta2Lat = (blockCoords[5] - blockCoords[3]) / nbTravees;

	        List<double[]> traveeCoords = new List<double[]>();

	        traveeCoords.Add(new double[2] {
		        blockCoords[0] + delta1Lon * numTravee, 
		        blockCoords[1] + delta1Lat * numTravee
	        });
	        traveeCoords.Add(new double[2]  {
		        blockCoords[2] + delta2Lon * numTravee, 
		        blockCoords[3] + delta2Lat * numTravee
	        });
	        traveeCoords.Add(new double[2]  {
		        blockCoords[2] + delta2Lon * (numTravee+1), 
		        blockCoords[3] + delta2Lat * (numTravee+1)
	        });
	        traveeCoords.Add(new double[2]  {
		        blockCoords[0] + delta1Lon * (numTravee+1), 
		        blockCoords[1] + delta1Lat * (numTravee+1)
	        });

	        return traveeCoords;
        }

        public List<double[]> getSlotCoords(Block block, List<double[]> traveeCoords, int numSlot) {
	        int nbSlots = block.getNbSlots();

            int firstSlot = block.getFirstSlot();
            numSlot -= firstSlot;

            // Inversion des slots
            numSlot = nbSlots - numSlot - 1;
	
	        double delta1Lon = (traveeCoords[1][0] - traveeCoords[0][0]) / nbSlots;
	        double delta1Lat = (traveeCoords[1][1] - traveeCoords[0][1]) / nbSlots;
	        double delta2Lon = (traveeCoords[2][0] - traveeCoords[3][0]) / nbSlots;
	        double delta2Lat = (traveeCoords[2][1] - traveeCoords[3][1]) / nbSlots;

	        List<double[]> slotCoords = new List<double[]>();
	
	        slotCoords.Add(new double[] {
		        traveeCoords[0][0] + delta1Lon * numSlot, 
		        traveeCoords[0][1] + delta1Lat * numSlot
	        });
	        slotCoords.Add(new double[] {
		        traveeCoords[0][0] + delta2Lon * (numSlot+1), 
		        traveeCoords[0][1] + delta2Lat * (numSlot+1)
	        });
	        slotCoords.Add(new double[] {
		        traveeCoords[3][0] + delta2Lon * (numSlot+1), 
		        traveeCoords[3][1] + delta2Lat * (numSlot+1)
	        });
	        slotCoords.Add(new double[] {
		        traveeCoords[3][0] + delta1Lon * numSlot, 
		        traveeCoords[3][1] + delta1Lat * numSlot
	        });

	        return slotCoords;
        }

        public List<double[]> getSlotCoords(Block block, int numTravee, int numSlot) {
	        return new List<double[]>();
        }

        //////////////
        // UTILS /////
        //////////////

        // 0 ==> Latitude
        // 1 ==> Longitude
        public static double[] getCenter(List<double> coordsList) {
	        double[] center = new double[2];

	        double signedArea = 0.0;
	
	        //A
	        signedArea=0.0;
	        for(int i=0; i<=4; i=i+2) signedArea+=coordsList[i]*coordsList[i+3]-coordsList[i+2]*coordsList[i+1];
	        signedArea+=coordsList[6]*coordsList[1]-coordsList[0]*coordsList[7];
	        signedArea*=0.5;
		
	        //Cx
	        center[1]=0.0;
	        for(int i=0; i<=4; i=i+2) center[1]+=(coordsList[i]+coordsList[i+2])*(coordsList[i]*coordsList[i+3]-coordsList[i+2]*coordsList[i+1]);
	        center[1]+=(coordsList[6]+coordsList[0])*(coordsList[6]*coordsList[1]-coordsList[0]*coordsList[7]);
	        center[1]/=(6*signedArea);

	        //Cy
	        center[0]=0.0;
	        for(int i=0; i<=4; i=i+2) center[0]+=(coordsList[i+1]+coordsList[i+3])*(coordsList[i]*coordsList[i+3]-coordsList[i+2]*coordsList[i+1]);
	        center[0]+=(coordsList[7]+coordsList[1])*(coordsList[6]*coordsList[1]-coordsList[0]*coordsList[7]);
	        center[0]/=(6*signedArea);

	        return center;
        }

    }
}
