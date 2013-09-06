using System;
using System.IO;
using Newtonsoft.Json;

namespace SolarCar {
	static class Json {
		public static T CreateFromJsonStream<T>(this Stream stream) {
			JsonSerializer serializer = new JsonSerializer();
			T data;
			using (StreamReader streamReader = new StreamReader(stream)) {
				data = (T)serializer.Deserialize(streamReader, typeof(T));
			}
			return data;
		}

		public static T CreateFromJsonString<T>(this String json) {
			T data;
			using (MemoryStream stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(json))) {
				data = CreateFromJsonStream<T>(stream);
			}
			return data;
		}

		public static T CreateFromJsonFile<T>(this String fileName) {
			T data;
			using (FileStream fileStream = new FileStream(fileName, FileMode.Open)) {
				data = CreateFromJsonStream<T>(fileStream);
			}
			return data;
		}
	}
}

