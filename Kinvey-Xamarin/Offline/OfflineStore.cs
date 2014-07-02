using System;

namespace KinveyXamarin
{
	public interface OfflineStore<T> {

		T executeGet();

		T executeSave();

		void executeDelete();

		void insertEntity();

		void clearStorage();

		void kickOffSync();

	}
}

