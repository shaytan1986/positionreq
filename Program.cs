using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace pssis
{
    class Program
    {
        private const string TEST_PROJECT_NUMBER = "19005";
        private const int MOCK_COUNT = 300000;
        static void Main(string[] args)
        {
            string projectNumber = args.Length > 0 ? args[0] : TEST_PROJECT_NUMBER;

            ProjectContext project = new ProjectContext(projectNumber);

            var TypedElements = project.GetTypedElements();

            foreach (var kvm in TypedElements)
            {
                var goodType = kvm.Key;

                JObject req = project.GenerateRequest(goodType, kvm.Value);

                Console.WriteLine(req.ToString());
            }
        }

    }

    class ProjectContext
    {
        public readonly string ProjectNumber;

        private IEnumerable<Element> elements;
        public Dictionary<Element.TypeOfGood, IEnumerable<Element>> GetTypedElements()
        {
            int enumSize = Enum.GetNames(typeof(Element.TypeOfGood)).Length;
            Dictionary<Element.TypeOfGood, IEnumerable<Element>> typedElements = 
                new Dictionary<Element.TypeOfGood, IEnumerable<Element>>();

            for (int i = 0; i < enumSize; i++)
            {
                Element.TypeOfGood goodType = (Element.TypeOfGood)i;

                var query = elements
                    .Where(e => e.GoodType == goodType)
                    .Select(e => e);

                typedElements[goodType] = query;
                
            }

            return typedElements;
            
        }
        public JObject GenerateRequest(Element.TypeOfGood goodType, IEnumerable<Element> elements)
        {
            
            return new JObject(
                new JProperty("EndPoint",
                    new JObject(
                        new JProperty("method", "post"),
                        new JProperty("Parameters", 
                            new JArray(
                                new JObject(
                                    new JProperty("name", "api-version"),
                                    new JProperty("type", null)
                                ),
                                new JObject(
                                    new JProperty("name", "environment"),
                                    new JProperty("type", "P1")
                                )
                            )
                        )
                    )
                ),
                new JProperty("query",
                    new JObject(
                        new JProperty("number", ProjectNumber),
                        new JProperty("names", SerializePositionsFor(goodType, elements))
                    )
                )
            );
        }

        public ProjectContext(string projectNumber)
        {
            ProjectNumber = projectNumber;
            elements = LoadElements();
        }

        public static implicit operator ProjectContext(string projectNumber) => new ProjectContext(projectNumber);

        private IEnumerable<Element> LoadElements() {
            return Element.GetMockElements();

        }

        public string SerializePositionsFor(Element.TypeOfGood goodType, IEnumerable<Element> elements)
        {
            
            var query = elements
                .Where(e => e.GoodType == goodType)
                .Select(e => e.Position);

            JArray positions = new JArray(query);

            return positions.ToString(Formatting.None);
            
        }

    }
    class Element
    {

        
        public Element(string position, TypeOfGood goodType)
        {
            Position = position;
            GoodType = goodType;
        }

        public enum TypeOfGood
        {
            Beams,
            Panels,
            Posts,
            Trusses,
            MetalDeck,
            BasePlates   
        }
        public readonly string Position;
        public readonly TypeOfGood GoodType;

        /// <summary>
        /// Gets a specified number of fake Elements with a Guid for the Position, and a FinishedGoodType
        /// derived from the modulo of the current index
        /// </summary>
        /// <param name="mockCount">Number of mocks to generate</param>
        /// <returns></returns>
        public static IEnumerable<Element> GetMockElements(int mockCount = 20)
        {
            List<Element> elements = new List<Element>();
            int enumSize = Enum.GetNames(typeof(TypeOfGood)).Length;

            for (int i = 0; i < mockCount; i++)
            {
                // evenly distribute the elements over the available mock types
                int mockTypeIndex = i % enumSize;
                TypeOfGood mockType = (TypeOfGood)mockTypeIndex;
                string mockPosition = Guid.NewGuid().ToString();

                elements.Add(new Element(mockPosition, mockType));
            }

            return elements;
        }
    }

    
}
